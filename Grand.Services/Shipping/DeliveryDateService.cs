using Grand.Core.Caching;
using Grand.Core.Caching.Constants;
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
    public class DeliveryDateService : IDeliveryDateService
    {
        #region Fields

        private readonly IRepository<DeliveryDate> _deliveryDateRepository;
        private readonly IMediator _mediator;
        private readonly ICacheBase _cacheBase;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public DeliveryDateService(
            IRepository<DeliveryDate> deliveryDateRepository,
            IMediator mediator,
            ICacheBase cacheManager)
        {
            _deliveryDateRepository = deliveryDateRepository;
            _mediator = mediator;
            _cacheBase = cacheManager;
        }

        #endregion

        #region Delivery dates

        /// <summary>
        /// Gets a delivery date
        /// </summary>
        /// <param name="deliveryDateId">The delivery date identifier</param>
        /// <returns>Delivery date</returns>
        public virtual Task<DeliveryDate> GetDeliveryDateById(string deliveryDateId)
        {
            string key = string.Format(CacheKey.DELIVERYDATE_BY_ID_KEY, deliveryDateId);
            return _cacheBase.GetAsync(key, () => _deliveryDateRepository.GetByIdAsync(deliveryDateId));
        }

        /// <summary>
        /// Gets all delivery dates
        /// </summary>
        /// <returns>Delivery dates</returns>
        public virtual async Task<IList<DeliveryDate>> GetAllDeliveryDates()
        {
            return await _cacheBase.GetAsync(CacheKey.DELIVERYDATE_ALL, () =>
            {
                var query = from dd in _deliveryDateRepository.Table
                            orderby dd.DisplayOrder
                            select dd;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Inserts a delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        public virtual async Task InsertDeliveryDate(DeliveryDate deliveryDate)
        {
            if (deliveryDate == null)
                throw new ArgumentNullException("deliveryDate");

            await _deliveryDateRepository.InsertAsync(deliveryDate);

            //event notification
            await _mediator.EntityInserted(deliveryDate);
        }

        /// <summary>
        /// Updates the delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        public virtual async Task UpdateDeliveryDate(DeliveryDate deliveryDate)
        {
            if (deliveryDate == null)
                throw new ArgumentNullException("deliveryDate");

            await _deliveryDateRepository.UpdateAsync(deliveryDate);

            //clear cache
            await _cacheBase.RemoveByPrefix(CacheKey.DELIVERYDATE_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(deliveryDate);
        }

        /// <summary>
        /// Deletes a delivery date
        /// </summary>
        /// <param name="deliveryDate">The delivery date</param>
        public virtual async Task DeleteDeliveryDate(DeliveryDate deliveryDate)
        {
            if (deliveryDate == null)
                throw new ArgumentNullException("deliveryDate");

            await _deliveryDateRepository.DeleteAsync(deliveryDate);

            //clear cache
            await _cacheBase.RemoveByPrefix(CacheKey.DELIVERYDATE_PATTERN_KEY);

            //clear product cache
            await _cacheBase.RemoveByPrefix(CacheKey.PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(deliveryDate);
        }

        #endregion

    }
}
