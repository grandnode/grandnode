using System;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using MongoDB.Driver;
using Grand.Plugin.Shipping.ShippingPoint.Domain;

namespace Grand.Plugin.Shipping.ShippingPoint.Services
{
    public class ShippingPointService : IShippingPointService
    {
        #region Constants

        private const string PICKUP_POINT_ALL_KEY = "Grand.ShippingPoint.all-{0}-{1}";
        private const string PICKUP_POINT_PATTERN_KEY = "Grand.ShippingPoint.";

        #endregion

        #region Fields

        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Domain.ShippingPoints> _shippingPointRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="ShippingPointRepository">Store pickup point repository</param>
        public ShippingPointService(ICacheManager cacheManager,
            IRepository<Domain.ShippingPoints> ShippingPointRepository)
        {
            this._cacheManager = cacheManager;
            this._shippingPointRepository = ShippingPointRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Pickup points</returns>
        public virtual IPagedList<ShippingPoints> GetAllStoreShippingPoint(string storeId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from gp in _shippingPointRepository.Table
                        where (gp.StoreId == storeId || string.IsNullOrEmpty(gp.StoreId)) || storeId == ""
                        select gp;
            var records = query.ToList();

            //paging
            return new PagedList<Domain.ShippingPoints>(records, pageIndex, pageSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pickupPointId"></param>
        /// <returns></returns>
        public virtual Domain.ShippingPoints GetStoreShippingPointByPointName(string pointName)
        {
            if (string.IsNullOrEmpty(pointName))
                return null;

            var query = (from shippingOoint in _shippingPointRepository.Table
                         where shippingOoint.ShippingPointName == pointName
                         select shippingOoint).SingleOrDefault();

            return query;
        }

        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointId">Pickup point identifier</param>
        /// <returns>Pickup point</returns>
        public virtual Domain.ShippingPoints GetStoreShippingPointById(string pickupPointId)
        {
            if (string.IsNullOrEmpty(pickupPointId))
                return null;

            return _shippingPointRepository.GetById(pickupPointId);
        }

        /// <summary>
        /// Inserts a pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        public virtual void InsertStoreShippingPoint(Domain.ShippingPoints pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            _shippingPointRepository.Insert(pickupPoint);
            _cacheManager.RemoveByPattern(PICKUP_POINT_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        public virtual void UpdateStoreShippingPoint(Domain.ShippingPoints pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            _shippingPointRepository.Update(pickupPoint);
            _cacheManager.RemoveByPattern(PICKUP_POINT_PATTERN_KEY);
        }

        /// <summary>
        /// Deletes a pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        public virtual void DeleteStoreShippingPoint(Domain.ShippingPoints pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            _shippingPointRepository.Delete(pickupPoint);
            _cacheManager.RemoveByPattern(PICKUP_POINT_PATTERN_KEY);
        }
        #endregion
    }
}
