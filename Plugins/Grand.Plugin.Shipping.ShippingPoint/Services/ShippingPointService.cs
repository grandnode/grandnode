using Grand.Core.Caching;
using Grand.Domain;
using Grand.Domain.Data;
using Grand.Plugin.Shipping.ShippingPoint.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Shipping.ShippingPoint.Services
{
    public class ShippingPointService : IShippingPointService
    {
        #region Constants

        private const string PICKUP_POINT_PATTERN_KEY = "Grand.ShippingPoint.";

        #endregion

        #region Fields

        private readonly ICacheManager _cacheManager;
        private readonly IRepository<ShippingPoints> _shippingPointRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="ShippingPointRepository">Store pickup point repository</param>
        public ShippingPointService(ICacheManager cacheManager,
            IRepository<ShippingPoints> ShippingPointRepository)
        {
            _cacheManager = cacheManager;
            _shippingPointRepository = ShippingPointRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <param name="storeId">The store identifier; pass "" to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Pickup points</returns>
        public virtual async Task<IPagedList<ShippingPoints>> GetAllStoreShippingPoint(string storeId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from gp in _shippingPointRepository.Table
                        where (gp.StoreId == storeId || string.IsNullOrEmpty(gp.StoreId)) || storeId == ""
                        select gp;

            var records = await query.ToListAsync();

            //paging
            return await Task.FromResult(new PagedList<ShippingPoints>(records, pageIndex, pageSize));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pickupPointId"></param>
        /// <returns></returns>
        public virtual Task<ShippingPoints> GetStoreShippingPointByPointName(string pointName)
        {
            return (from shippingOoint in _shippingPointRepository.Table
                    where shippingOoint.ShippingPointName == pointName
                    select shippingOoint).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointId">Pickup point identifier</param>
        /// <returns>Pickup point</returns>
        public virtual Task<ShippingPoints> GetStoreShippingPointById(string pickupPointId)
        {
            return _shippingPointRepository.GetByIdAsync(pickupPointId);
        }

        /// <summary>
        /// Inserts a pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        public virtual async Task InsertStoreShippingPoint(ShippingPoints pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            await _shippingPointRepository.InsertAsync(pickupPoint);
            await _cacheManager.RemoveByPrefix(PICKUP_POINT_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        public virtual async Task UpdateStoreShippingPoint(ShippingPoints pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            await _shippingPointRepository.UpdateAsync(pickupPoint);
            await _cacheManager.RemoveByPrefix(PICKUP_POINT_PATTERN_KEY);
        }

        /// <summary>
        /// Deletes a pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        public virtual async Task DeleteStoreShippingPoint(ShippingPoints pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            await _shippingPointRepository.DeleteAsync(pickupPoint);
            await _cacheManager.RemoveByPrefix(PICKUP_POINT_PATTERN_KEY);
        }
        #endregion
    }
}
