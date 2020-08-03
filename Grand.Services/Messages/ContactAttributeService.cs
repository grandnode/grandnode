using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Messages;
using Grand.Services.Customers;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    /// <summary>
    /// Contact attribute service
    /// </summary>
    public partial class ContactAttributeService : IContactAttributeService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// {1} : ignore ACL?
        /// </remarks>
        private const string CONTACTATTRIBUTES_ALL_KEY = "Grand.contactattribute.all-{0}-{1}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : contact attribute ID
        /// </remarks>
        private const string CONTACTATTRIBUTES_BY_ID_KEY = "Grand.contactattribute.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CONTACTATTRIBUTES_PATTERN_KEY = "Grand.contactattribute.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CONTACTATTRIBUTEVALUES_PATTERN_KEY = "Grand.contactattributevalue.";
        #endregion
        
        #region Fields

        private readonly IRepository<ContactAttribute> _contactAttributeRepository;
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
        /// <param name="contactAttributeRepository">Contact attribute repository</param>
        /// <param name="mediator">Mediator</param>
        public ContactAttributeService(ICacheManager cacheManager,
            IRepository<ContactAttribute> contactAttributeRepository,
            IMediator mediator,
            IWorkContext workContext,
            CatalogSettings catalogSettings)
        {
            _cacheManager = cacheManager;
            _contactAttributeRepository = contactAttributeRepository;
            _mediator = mediator;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        #region Contact attributes

        /// <summary>
        /// Deletes a contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        public virtual async Task DeleteContactAttribute(ContactAttribute contactAttribute)
        {
            if (contactAttribute == null)
                throw new ArgumentNullException("contactAttribute");

            await _contactAttributeRepository.DeleteAsync(contactAttribute);

            await _cacheManager.RemoveByPrefix(CONTACTATTRIBUTES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CONTACTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(contactAttribute);
        }

        /// <summary>
        /// Gets all contact attributes
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Contact attributes</returns>
        public virtual async Task<IList<ContactAttribute>> GetAllContactAttributes(string storeId = "", bool ignorAcl = false)
        {
            string key = string.Format(CONTACTATTRIBUTES_ALL_KEY, storeId, ignorAcl);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = _contactAttributeRepository.Table;
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
                return query.ToListAsync();

            });
        }

        /// <summary>
        /// Gets a contact attribute 
        /// </summary>
        /// <param name="contactAttributeId">Contact attribute identifier</param>
        /// <returns>Contact attribute</returns>
        public virtual Task<ContactAttribute> GetContactAttributeById(string contactAttributeId)
        {
            string key = string.Format(CONTACTATTRIBUTES_BY_ID_KEY, contactAttributeId);
            return _cacheManager.GetAsync(key, () => _contactAttributeRepository.GetByIdAsync(contactAttributeId));
        }

        /// <summary>
        /// Inserts a contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        public virtual async Task InsertContactAttribute(ContactAttribute contactAttribute)
        {
            if (contactAttribute == null)
                throw new ArgumentNullException("contactAttribute");

            await _contactAttributeRepository.InsertAsync(contactAttribute);

            await _cacheManager.RemoveByPrefix(CONTACTATTRIBUTES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CONTACTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(contactAttribute);
        }

        /// <summary>
        /// Updates the contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        public virtual async Task UpdateContactAttribute(ContactAttribute contactAttribute)
        {
            if (contactAttribute == null)
                throw new ArgumentNullException("contactAttribute");

            await _contactAttributeRepository.UpdateAsync(contactAttribute);

            await _cacheManager.RemoveByPrefix(CONTACTATTRIBUTES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CONTACTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(contactAttribute);
        }

        #endregion


        #endregion
    }
}
