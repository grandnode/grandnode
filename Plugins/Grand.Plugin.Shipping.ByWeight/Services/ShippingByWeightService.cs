using Grand.Core.Caching;
using Grand.Domain;
using Grand.Domain.Data;
using Grand.Plugin.Shipping.ByWeight.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Shipping.ByWeight.Services
{
    public partial class ShippingByWeightService : IShippingByWeightService
    {
        #region Constants
        private const string SHIPPINGBYWEIGHT_ALL_KEY = "Grand.shippingbyweight.all-{0}-{1}";
        private const string SHIPPINGBYWEIGHT_PATTERN_KEY = "Grand.shippingbyweight.";
        #endregion

        #region Fields

        private readonly IRepository<ShippingByWeightRecord> _sbwRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public ShippingByWeightService(ICacheManager cacheManager,
            IRepository<ShippingByWeightRecord> sbwRepository)
        {
            _cacheManager = cacheManager;
            _sbwRepository = sbwRepository;
        }

        #endregion

        #region Methods

        public virtual async Task DeleteShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            await _sbwRepository.DeleteAsync(shippingByWeightRecord);

            await _cacheManager.RemoveByPrefix(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }

        public virtual async Task<IPagedList<ShippingByWeightRecord>> GetAll(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            string key = string.Format(SHIPPINGBYWEIGHT_ALL_KEY, pageIndex, pageSize);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = from sbw in _sbwRepository.Table
                            orderby sbw.StoreId, sbw.CountryId, sbw.StateProvinceId, sbw.Zip, sbw.ShippingMethodId, sbw.From
                            select sbw;

                return Task.FromResult(new PagedList<ShippingByWeightRecord>(query, pageIndex, pageSize));
            });
        }

        public virtual async Task<ShippingByWeightRecord> FindRecord(string shippingMethodId,
            string storeId, string warehouseId,
            string countryId, string stateProvinceId, string zip, decimal weight)
        {
            if (zip == null)
                zip = string.Empty;
            zip = zip.Trim();

            //filter by weight and shipping method
            var existingRates = (await GetAll())
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
                    (zip.Equals(sbw.Zip, StringComparison.OrdinalIgnoreCase)))
                    matchedByZip.Add(sbw);

            if (matchedByZip.Count == 0)
                foreach (var taxRate in matchedByStateProvince)
                    if (String.IsNullOrWhiteSpace(taxRate.Zip))
                        matchedByZip.Add(taxRate);

            return matchedByZip.FirstOrDefault();
        }

        public virtual Task<ShippingByWeightRecord> GetById(string shippingByWeightRecordId)
        {
            return _sbwRepository.GetByIdAsync(shippingByWeightRecordId);
        }

        public virtual async Task InsertShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            await _sbwRepository.InsertAsync(shippingByWeightRecord);

            await _cacheManager.RemoveByPrefix(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }

        public virtual async Task UpdateShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            await _sbwRepository.UpdateAsync(shippingByWeightRecord);

            await _cacheManager.RemoveByPrefix(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }

        #endregion
    }

}
