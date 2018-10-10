using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Messages;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Stores;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : contact attribute ID
        /// </remarks>
        private const string CONTACTATTRIBUTEVALUES_ALL_KEY = "Grand.contactattributevalue.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : contact attribute value ID
        /// </remarks>
        private const string CONTACTATTRIBUTEVALUES_BY_ID_KEY = "Grand.contactattributevalue.id-{0}";
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
        private readonly IStoreMappingService _storeMappingService;
        private readonly IEventPublisher _eventPublisher;
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
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="eventPublisher">Event published</param>
        public ContactAttributeService(ICacheManager cacheManager,
            IRepository<ContactAttribute> contactAttributeRepository,
            IStoreMappingService storeMappingService,
            IEventPublisher eventPublisher,
            IWorkContext workContext,
            CatalogSettings catalogSettings)
        {
            this._cacheManager = cacheManager;
            this._contactAttributeRepository = contactAttributeRepository;
            this._storeMappingService = storeMappingService;
            this._eventPublisher = eventPublisher;
            this._workContext = workContext;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        #region Contact attributes

        /// <summary>
        /// Deletes a contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        public virtual void DeleteContactAttribute(ContactAttribute contactAttribute)
        {
            if (contactAttribute == null)
                throw new ArgumentNullException("contactAttribute");

            _contactAttributeRepository.Delete(contactAttribute);

            _cacheManager.RemoveByPattern(CONTACTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CONTACTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(contactAttribute);
        }

        /// <summary>
        /// Gets all contact attributes
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="excludeShippableAttributes">A value indicating whether we should exlude shippable attributes</param>
        /// <returns>Contact attributes</returns>
        public virtual IList<ContactAttribute> GetAllContactAttributes(string storeId = "", bool ignorAcl = false)
        {
            string key = string.Format(CONTACTATTRIBUTES_ALL_KEY, storeId, ignorAcl);
            return _cacheManager.Get(key, () =>
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
                return query.ToList();

            });
        }

        /// <summary>
        /// Gets a contact attribute 
        /// </summary>
        /// <param name="contactAttributeId">Contact attribute identifier</param>
        /// <returns>Contact attribute</returns>
        public virtual ContactAttribute GetContactAttributeById(string contactAttributeId)
        {
            string key = string.Format(CONTACTATTRIBUTES_BY_ID_KEY, contactAttributeId);
            return _cacheManager.Get(key, () => _contactAttributeRepository.GetById(contactAttributeId));
        }

        /// <summary>
        /// Inserts a contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        public virtual void InsertContactAttribute(ContactAttribute contactAttribute)
        {
            if (contactAttribute == null)
                throw new ArgumentNullException("contactAttribute");

            _contactAttributeRepository.Insert(contactAttribute);

            _cacheManager.RemoveByPattern(CONTACTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CONTACTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(contactAttribute);
        }

        /// <summary>
        /// Updates the contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        public virtual void UpdateContactAttribute(ContactAttribute contactAttribute)
        {
            if (contactAttribute == null)
                throw new ArgumentNullException("contactAttribute");

            _contactAttributeRepository.Update(contactAttribute);

            _cacheManager.RemoveByPattern(CONTACTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CONTACTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(contactAttribute);
        }

        #endregion


        #endregion
    }
}
