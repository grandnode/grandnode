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
    public class PickupPointService : IPickupPointService
    {

        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : picpup point ID
        /// </remarks>
        private const string PICKUPPOINTS_BY_ID_KEY = "Grand.pickuppoint.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string PICKUPPOINTS_ALL = "Grand.pickuppoint.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PICKUPPOINTS_PATTERN_KEY = "Grand.pickuppoint.";

        #endregion

        #region Fields

        private readonly IRepository<PickupPoint> _pickupPointsRepository;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public PickupPointService(
            IRepository<PickupPoint> pickupPointsRepository,
            IMediator mediator,
            ICacheManager cacheManager)
        {
            _pickupPointsRepository = pickupPointsRepository;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointId">The pickup point identifier</param>
        /// <returns>Delivery date</returns>
        public virtual Task<PickupPoint> GetPickupPointById(string pickupPointId)
        {
            var key = string.Format(PICKUPPOINTS_BY_ID_KEY, pickupPointId);
            return _cacheManager.GetAsync(key, () => _pickupPointsRepository.GetByIdAsync(pickupPointId));
        }

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual async Task<IList<PickupPoint>> GetAllPickupPoints()
        {
            return await _cacheManager.GetAsync(PICKUPPOINTS_ALL, () =>
            {
                var query = from pp in _pickupPointsRepository.Table
                            orderby pp.DisplayOrder
                            select pp;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual async Task<IList<PickupPoint>> LoadActivePickupPoints(string storeId = "")
        {
            var pickupPoints = await GetAllPickupPoints();
            return pickupPoints.Where(pp => pp.StoreId == storeId || string.IsNullOrEmpty(pp.StoreId)).ToList();
        }


        /// <summary>
        /// Inserts a warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual async Task InsertPickupPoint(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            await _pickupPointsRepository.InsertAsync(pickupPoint);

            //clear cache
            await _cacheManager.RemoveByPrefix(PICKUPPOINTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(pickupPoint);
        }

        /// <summary>
        /// Updates the warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual async Task UpdatePickupPoint(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            await _pickupPointsRepository.UpdateAsync(pickupPoint);

            //clear cache
            await _cacheManager.RemoveByPrefix(PICKUPPOINTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(pickupPoint);
        }

        /// <summary>
        /// Deletes a delivery date
        /// </summary>
        /// <param name="deliveryDate">The delivery date</param>
        public virtual async Task DeletePickupPoint(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            await _pickupPointsRepository.DeleteAsync(pickupPoint);

            //clear cache
            await _cacheManager.RemoveByPrefix(PICKUPPOINTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(pickupPoint);
        }


        #endregion
    }
}
