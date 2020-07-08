using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Orders;
using Grand.Services.Customers;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Checkout attribute service
    /// </summary>
    public partial class CheckoutAttributeService : ICheckoutAttributeService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// {1} : >A value indicating whether we should exlude shippable attributes
        /// {2} : ignore ACL?
        /// </remarks>
        private const string CHECKOUTATTRIBUTES_ALL_KEY = "Grand.checkoutattribute.all-{0}-{1}-{2}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : checkout attribute ID
        /// </remarks>
        private const string CHECKOUTATTRIBUTES_BY_ID_KEY = "Grand.checkoutattribute.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CHECKOUTATTRIBUTES_PATTERN_KEY = "Grand.checkoutattribute.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CHECKOUTATTRIBUTEVALUES_PATTERN_KEY = "Grand.checkoutattributevalue.";
        #endregion
        
        #region Fields

        private readonly IRepository<CheckoutAttribute> _checkoutAttributeRepository;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="checkoutAttributeRepository">Checkout attribute repository</param>
        /// <param name="mediator">Mediator</param>
        public CheckoutAttributeService(ICacheManager cacheManager,
            IRepository<CheckoutAttribute> checkoutAttributeRepository,
            IMediator mediator,
            IWorkContext workContext,
            CatalogSettings catalogSettings)
        {
            _cacheManager = cacheManager;
            _checkoutAttributeRepository = checkoutAttributeRepository;
            _mediator = mediator;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        public virtual async Task DeleteCheckoutAttribute(CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                throw new ArgumentNullException("checkoutAttribute");

            await _checkoutAttributeRepository.DeleteAsync(checkoutAttribute);

            await _cacheManager.RemoveByPrefix(CHECKOUTATTRIBUTES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CHECKOUTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(checkoutAttribute);
        }

        /// <summary>
        /// Gets all checkout attributes
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="excludeShippableAttributes">A value indicating whether we should exlude shippable attributes</param>
        /// <returns>Checkout attributes</returns>
        public virtual async Task<IList<CheckoutAttribute>> GetAllCheckoutAttributes(string storeId = "", bool excludeShippableAttributes = false, bool ignorAcl = false)
        {
            string key = string.Format(CHECKOUTATTRIBUTES_ALL_KEY, storeId, excludeShippableAttributes, ignorAcl);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = _checkoutAttributeRepository.Table;
                query = query.OrderBy(c => c.DisplayOrder);

                if ((!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations) ||
                    (!ignorAcl && !_catalogSettings.IgnoreAcl))
                {
                    if (!ignorAcl && !_catalogSettings.IgnoreAcl)
                    {
                        var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                        query = from p in query
                                where !p.SubjectToAcl || allowedCustomerRolesIds.Any(x => p.CustomerRoles.Contains(x))
                                select p;
                    }
                    //Store mapping
                    if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
                    {
                        query = from p in query
                                where !p.LimitedToStores || p.Stores.Contains(storeId)
                                select p; 
                    }
                }
                if (excludeShippableAttributes)
                {
                    query = query.Where(x => !x.ShippableProductRequired);
                }
                return query.ToListAsync();

            });
        }

        /// <summary>
        /// Gets a checkout attribute 
        /// </summary>
        /// <param name="checkoutAttributeId">Checkout attribute identifier</param>
        /// <returns>Checkout attribute</returns>
        public virtual Task<CheckoutAttribute> GetCheckoutAttributeById(string checkoutAttributeId)
        {
            string key = string.Format(CHECKOUTATTRIBUTES_BY_ID_KEY, checkoutAttributeId);
            return _cacheManager.GetAsync(key, () => _checkoutAttributeRepository.GetByIdAsync(checkoutAttributeId));
        }

        /// <summary>
        /// Inserts a checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        public virtual async Task InsertCheckoutAttribute(CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                throw new ArgumentNullException("checkoutAttribute");

            await _checkoutAttributeRepository.InsertAsync(checkoutAttribute);

            await _cacheManager.RemoveByPrefix(CHECKOUTATTRIBUTES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CHECKOUTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(checkoutAttribute);
        }

        /// <summary>
        /// Updates the checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        public virtual async Task UpdateCheckoutAttribute(CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                throw new ArgumentNullException("checkoutAttribute");

            await _checkoutAttributeRepository.UpdateAsync(checkoutAttribute);

            await _cacheManager.RemoveByPrefix(CHECKOUTATTRIBUTES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CHECKOUTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(checkoutAttribute);
        }

        #endregion
    }
}
