using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Services.Events;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer attribute service
    /// </summary>
    public partial class CustomerAttributeService : ICustomerAttributeService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string CUSTOMERATTRIBUTES_ALL_KEY = "Grand.customerattribute.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer attribute ID
        /// </remarks>
        private const string CUSTOMERATTRIBUTES_BY_ID_KEY = "Grand.customerattribute.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer attribute ID
        /// </remarks>
        private const string CUSTOMERATTRIBUTEVALUES_ALL_KEY = "Grand.customerattributevalue.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer attribute value ID
        /// </remarks>
        private const string CUSTOMERATTRIBUTEVALUES_BY_ID_KEY = "Grand.customerattributevalue.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CUSTOMERATTRIBUTES_PATTERN_KEY = "Grand.customerattribute.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CUSTOMERATTRIBUTEVALUES_PATTERN_KEY = "Grand.customerattributevalue.";
        #endregion
        
        #region Fields

        private readonly IRepository<CustomerAttribute> _customerAttributeRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="customerAttributeRepository">Customer attribute repository</param>
        /// <param name="customerAttributeValueRepository">Customer attribute value repository</param>
        /// <param name="eventPublisher">Event published</param>
        public CustomerAttributeService(ICacheManager cacheManager,
            IRepository<CustomerAttribute> customerAttributeRepository,            
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._customerAttributeRepository = customerAttributeRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a customer attribute
        /// </summary>
        /// <param name="customerAttribute">Customer attribute</param>
        public virtual async Task DeleteCustomerAttribute(CustomerAttribute customerAttribute)
        {
            if (customerAttribute == null)
                throw new ArgumentNullException("customerAttribute");

            await _customerAttributeRepository.DeleteAsync(customerAttribute);

            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(customerAttribute);
        }

        /// <summary>
        /// Gets all customer attributes
        /// </summary>
        /// <returns>Customer attributes</returns>
        public virtual async Task<IList<CustomerAttribute>> GetAllCustomerAttributes()
        {
            string key = CUSTOMERATTRIBUTES_ALL_KEY;
            return await _cacheManager.Get(key, () =>
            {
                var query = from ca in _customerAttributeRepository.Table
                            orderby ca.DisplayOrder
                            select ca;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Gets a customer attribute 
        /// </summary>
        /// <param name="customerAttributeId">Customer attribute identifier</param>
        /// <returns>Customer attribute</returns>
        public virtual Task<CustomerAttribute> GetCustomerAttributeById(string customerAttributeId)
        {
            string key = string.Format(CUSTOMERATTRIBUTES_BY_ID_KEY, customerAttributeId);
            return _cacheManager.Get(key, () => _customerAttributeRepository.GetByIdAsync(customerAttributeId));
        }

        /// <summary>
        /// Inserts a customer attribute
        /// </summary>
        /// <param name="customerAttribute">Customer attribute</param>
        public virtual async Task InsertCustomerAttribute(CustomerAttribute customerAttribute)
        {
            if (customerAttribute == null)
                throw new ArgumentNullException("customerAttribute");

            await _customerAttributeRepository.InsertAsync(customerAttribute);

            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(customerAttribute);
        }

        /// <summary>
        /// Updates the customer attribute
        /// </summary>
        /// <param name="customerAttribute">Customer attribute</param>
        public virtual async Task UpdateCustomerAttribute(CustomerAttribute customerAttribute)
        {
            if (customerAttribute == null)
                throw new ArgumentNullException("customerAttribute");

            await _customerAttributeRepository.UpdateAsync(customerAttribute);

            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(customerAttribute);
        }

        /// <summary>
        /// Deletes a customer attribute value
        /// </summary>
        /// <param name="customerAttributeValue">Customer attribute value</param>
        public virtual async Task DeleteCustomerAttributeValue(CustomerAttributeValue customerAttributeValue)
        {
            if (customerAttributeValue == null)
                throw new ArgumentNullException("customerAttributeValue");

            var updatebuilder = Builders<CustomerAttribute>.Update;
            var update = updatebuilder.Pull(p => p.CustomerAttributeValues, customerAttributeValue);
            await _customerAttributeRepository.Collection.UpdateOneAsync(new BsonDocument("_id", customerAttributeValue.CustomerAttributeId), update);

            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(customerAttributeValue);
        }

        /// <summary>
        /// Inserts a customer attribute value
        /// </summary>
        /// <param name="customerAttributeValue">Customer attribute value</param>
        public virtual async Task InsertCustomerAttributeValue(CustomerAttributeValue customerAttributeValue)
        {
            if (customerAttributeValue == null)
                throw new ArgumentNullException("customerAttributeValue");

            var updatebuilder = Builders<CustomerAttribute>.Update;
            var update = updatebuilder.AddToSet(p => p.CustomerAttributeValues, customerAttributeValue);
            await _customerAttributeRepository.Collection.UpdateOneAsync(new BsonDocument("_id", customerAttributeValue.CustomerAttributeId), update);

            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(customerAttributeValue);
        }

        /// <summary>
        /// Updates the customer attribute value
        /// </summary>
        /// <param name="customerAttributeValue">Customer attribute value</param>
        public virtual async Task UpdateCustomerAttributeValue(CustomerAttributeValue customerAttributeValue)
        {
            if (customerAttributeValue == null)
                throw new ArgumentNullException("customerAttributeValue");

            var builder = Builders<CustomerAttribute>.Filter;
            var filter = builder.Eq(x => x.Id, customerAttributeValue.CustomerAttributeId);
            filter = filter & builder.ElemMatch(x => x.CustomerAttributeValues, y => y.Id == customerAttributeValue.Id);
            var update = Builders<CustomerAttribute>.Update
                .Set(x => x.CustomerAttributeValues.ElementAt(-1).DisplayOrder, customerAttributeValue.DisplayOrder)
                .Set(x => x.CustomerAttributeValues.ElementAt(-1).IsPreSelected, customerAttributeValue.IsPreSelected)
                .Set(x => x.CustomerAttributeValues.ElementAt(-1).Locales, customerAttributeValue.Locales)
                .Set(x => x.CustomerAttributeValues.ElementAt(-1).Name, customerAttributeValue.Name);

            await _customerAttributeRepository.Collection.UpdateManyAsync(filter, update);

            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CUSTOMERATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(customerAttributeValue);
        }
        
        #endregion
    }
}
