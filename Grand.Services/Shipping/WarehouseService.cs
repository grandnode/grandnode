using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Shipping;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Shipping
{
    public class WarehouseService : IWarehouseService
    {

        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : warehouse ID
        /// </remarks>
        private const string WAREHOUSES_BY_ID_KEY = "Grand.warehouse.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string WAREHOUSES_ALL = "Grand.warehouse.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string WAREHOUSES_PATTERN_KEY = "Grand.warehouse.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_PATTERN_KEY = "Grand.product.";

        #endregion

        #region Fields

        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public WarehouseService(
            IRepository<Warehouse> warehouseRepository,
            IMediator mediator,
            ICacheManager cacheManager)
        {
            _warehouseRepository = warehouseRepository;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Warehouses

        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="warehouseId">The warehouse identifier</param>
        /// <returns>Warehouse</returns>
        public virtual Task<Warehouse> GetWarehouseById(string warehouseId)
        {
            string key = string.Format(WAREHOUSES_BY_ID_KEY, warehouseId);
            return _cacheManager.GetAsync(key, () => _warehouseRepository.GetByIdAsync(warehouseId));
        }

        /// <summary>
        /// Gets all warehouses
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual async Task<IList<Warehouse>> GetAllWarehouses()
        {
            return await _cacheManager.GetAsync(WAREHOUSES_ALL, () =>
            {
                var query = from wh in _warehouseRepository.Table
                            orderby wh.DisplayOrder
                            select wh;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Inserts a warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual async Task InsertWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            await _warehouseRepository.InsertAsync(warehouse);

            //clear cache
            await _cacheManager.RemoveByPrefix(WAREHOUSES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(warehouse);
        }

        /// <summary>
        /// Updates the warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual async Task UpdateWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            await _warehouseRepository.UpdateAsync(warehouse);

            //clear cache
            await _cacheManager.RemoveByPrefix(WAREHOUSES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(warehouse);
        }

        /// <summary>
        /// Deletes a warehouse
        /// </summary>
        /// <param name="warehouse">The warehouse</param>
        public virtual async Task DeleteWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            await _warehouseRepository.DeleteAsync(warehouse);

            //clear cache
            await _cacheManager.RemoveByPrefix(WAREHOUSES_PATTERN_KEY);
            //clear product cache
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(warehouse);
        }
        #endregion
    }
}
