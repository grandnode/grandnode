using Grand.Core.Caching;
using Grand.Domain.Customers;
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
    public class ShippingMethodService : IShippingMethodService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : shippingmethod ID
        /// </remarks>
        private const string SHIPPINGMETHOD_BY_ID_KEY = "Grand.shippingmethod.id-{0}";

        /// <summary>
        /// Key to cache
        /// </summary>
        private const string SHIPPINGMETHOD_ALL = "Grand.shippingmethod.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string SHIPPINGMETHOD_PATTERN_KEY = "Grand.shippingmethod.";

        #endregion

        #region Fields

        private readonly IRepository<ShippingMethod> _shippingMethodRepository;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public ShippingMethodService(
            IRepository<ShippingMethod> shippingMethodRepository,
            IMediator mediator,
            ICacheManager cacheManager)
        {
            _shippingMethodRepository = shippingMethodRepository;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Shipping methods


        /// <summary>
        /// Deletes a shipping method
        /// </summary>
        /// <param name="shippingMethod">The shipping method</param>
        public virtual async Task DeleteShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            await _shippingMethodRepository.DeleteAsync(shippingMethod);

            //clear cache
            await _cacheManager.RemoveByPrefix(SHIPPINGMETHOD_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(shippingMethod);
        }

        /// <summary>
        /// Gets a shipping method
        /// </summary>
        /// <param name="shippingMethodId">The shipping method identifier</param>
        /// <returns>Shipping method</returns>
        public virtual Task<ShippingMethod> GetShippingMethodById(string shippingMethodId)
        {
            string key = string.Format(SHIPPINGMETHOD_BY_ID_KEY, shippingMethodId);
            return _cacheManager.GetAsync(key, () => _shippingMethodRepository.GetByIdAsync(shippingMethodId));
        }

        /// <summary>
        /// Gets all shipping methods
        /// </summary>
        /// <param name="filterByCountryId">The country indentifier to filter by</param>
        /// <returns>Shipping methods</returns>
        public virtual async Task<IList<ShippingMethod>> GetAllShippingMethods(string filterByCountryId = "", Customer customer = null)
        {
            var shippingMethods = new List<ShippingMethod>();

            shippingMethods = await _cacheManager.GetAsync(SHIPPINGMETHOD_ALL, () =>
            {
                var query = from sm in _shippingMethodRepository.Table
                            orderby sm.DisplayOrder
                            select sm;
                return query.ToListAsync();
            });

            if (!string.IsNullOrEmpty(filterByCountryId))
            {
                shippingMethods = shippingMethods.Where(x => !x.CountryRestrictionExists(filterByCountryId)).ToList();
            }
            if (customer != null)
            {
                shippingMethods = shippingMethods.Where(x => !x.CustomerRoleRestrictionExists(customer.CustomerRoles.Select(y => y.Id).ToList())).ToList();
            }

            return shippingMethods;
        }

        /// <summary>
        /// Inserts a shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        public virtual async Task InsertShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            await _shippingMethodRepository.InsertAsync(shippingMethod);

            //clear cache
            await _cacheManager.RemoveByPrefix(SHIPPINGMETHOD_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(shippingMethod);
        }

        /// <summary>
        /// Updates the shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        public virtual async Task UpdateShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            await _shippingMethodRepository.UpdateAsync(shippingMethod);

            //clear cache
            await _cacheManager.RemoveByPrefix(SHIPPINGMETHOD_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(shippingMethod);
        }

        #endregion
    }
}
