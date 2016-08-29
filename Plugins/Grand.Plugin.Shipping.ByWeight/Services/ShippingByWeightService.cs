using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Plugin.Shipping.ByWeight.Domain;

namespace Grand.Plugin.Shipping.ByWeight.Services
{
    public partial class ShippingByWeightService : IShippingByWeightService
    {
        #region Constants
        private const string SHIPPINGBYWEIGHT_ALL_KEY = "Nop.shippingbyweight.all-{0}-{1}";
        private const string SHIPPINGBYWEIGHT_PATTERN_KEY = "Nop.shippingbyweight.";
        #endregion

        #region Fields

        private readonly IRepository<ShippingByWeightRecord> _sbwRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public ShippingByWeightService(ICacheManager cacheManager,
            IRepository<ShippingByWeightRecord> sbwRepository)
        {
            this._cacheManager = cacheManager;
            this._sbwRepository = sbwRepository;
        }

        #endregion

        #region Methods

        public virtual void DeleteShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            _sbwRepository.Delete(shippingByWeightRecord);

            _cacheManager.RemoveByPattern(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }

        public virtual IPagedList<ShippingByWeightRecord> GetAll(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            string key = string.Format(SHIPPINGBYWEIGHT_ALL_KEY, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                var query = from sbw in _sbwRepository.Table
                            orderby sbw.StoreId, sbw.CountryId, sbw.StateProvinceId, sbw.Zip, sbw.ShippingMethodId, sbw.From
                            select sbw;

                var records = new PagedList<ShippingByWeightRecord>(query, pageIndex, pageSize);
                return records;
            });
        }

        public virtual ShippingByWeightRecord FindRecord(string shippingMethodId,
            string storeId, string warehouseId, 
            string countryId, string stateProvinceId, string zip, decimal weight)
        {
            if (zip == null)
                zip = string.Empty;
            zip = zip.Trim();

            //filter by weight and shipping method
            var existingRates = GetAll()
                .Where(sbw => sbw.ShippingMethodId == shippingMethodId && weight >= sbw.From && weight <= sbw.To)
                .ToList();

            //filter by store
            var matchedByStore = new List<ShippingByWeightRecord>();
            foreach (var sbw in existingRates)
                if (storeId == sbw.StoreId)
                    matchedByStore.Add(sbw);
            if (matchedByStore.Count == 0)
                foreach (var sbw in existingRates)
                    if (String.IsNullOrEmpty(sbw.StoreId))
                        matchedByStore.Add(sbw);

            //filter by warehouse
            var matchedByWarehouse = new List<ShippingByWeightRecord>();
            foreach (var sbw in matchedByStore)
                if (warehouseId == sbw.WarehouseId)
                    matchedByWarehouse.Add(sbw);
            if (matchedByWarehouse.Count == 0)
                foreach (var sbw in matchedByStore)
                    if (String.IsNullOrEmpty(sbw.WarehouseId))
                        matchedByWarehouse.Add(sbw);

            //filter by country
            var matchedByCountry = new List<ShippingByWeightRecord>();
            foreach (var sbw in matchedByWarehouse)
                if (countryId == sbw.CountryId)
                    matchedByCountry.Add(sbw);
            if (matchedByCountry.Count == 0)
                foreach (var sbw in matchedByWarehouse)
                    if (String.IsNullOrEmpty(sbw.CountryId))
                        matchedByCountry.Add(sbw);

            //filter by state/province
            var matchedByStateProvince = new List<ShippingByWeightRecord>();
            foreach (var sbw in matchedByCountry)
                if (stateProvinceId == sbw.StateProvinceId)
                    matchedByStateProvince.Add(sbw);
            if (matchedByStateProvince.Count == 0)
                foreach (var sbw in matchedByCountry)
                    if (String.IsNullOrEmpty(sbw.StateProvinceId))
                        matchedByStateProvince.Add(sbw);


            //filter by zip
            var matchedByZip = new List<ShippingByWeightRecord>();
            foreach (var sbw in matchedByStateProvince)
                if ((String.IsNullOrEmpty(zip) && String.IsNullOrEmpty(sbw.Zip)) ||
                    (zip.Equals(sbw.Zip, StringComparison.InvariantCultureIgnoreCase)))
                    matchedByZip.Add(sbw);

            if (matchedByZip.Count == 0)
                foreach (var taxRate in matchedByStateProvince)
                    if (String.IsNullOrWhiteSpace(taxRate.Zip))
                        matchedByZip.Add(taxRate);

            return matchedByZip.FirstOrDefault();
        }

        public virtual ShippingByWeightRecord GetById(string shippingByWeightRecordId)
        {
            if (String.IsNullOrEmpty(shippingByWeightRecordId))
                return null;

            return _sbwRepository.GetById(shippingByWeightRecordId);
        }

        public virtual void InsertShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            _sbwRepository.Insert(shippingByWeightRecord);

            _cacheManager.RemoveByPattern(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }

        public virtual void UpdateShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            _sbwRepository.Update(shippingByWeightRecord);

            _cacheManager.RemoveByPattern(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }

        #endregion
    }
}
