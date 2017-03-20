using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Common;
using Grand.Services.Events;
using MongoDB.Driver;
using MongoDB.Bson;
using Grand.Core;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer tag service
    /// </summary>
    public partial class CustomerTagService : ICustomerTagService
    {
        

        #region Fields

        private readonly IRepository<CustomerTag> _customerTagRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly CommonSettings _commonSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customerTagRepository">Customer tag repository</param>
        /// <param name="commonSettings">Common settings</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="eventPublisher">Event published</param>
        public CustomerTagService(IRepository<CustomerTag> customerTagRepository,
            IRepository<Customer> customerRepository,
            CommonSettings commonSettings,
            IEventPublisher eventPublisher
            )
        {
            this._customerTagRepository = customerTagRepository;
            this._commonSettings = commonSettings;
            this._eventPublisher = eventPublisher;
            this._customerRepository = customerRepository;
        }

        #endregion

        /// <summary>
        /// Gets all customer for tag id
        /// </summary>
        /// <returns>Customers</returns>
        public virtual IPagedList<Customer> GetCustomersByTag(string customerTagId = "", int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = from c in _customerRepository.Table
                        where c.CustomerTags.Contains(customerTagId)
                        select c;
            var customers = new PagedList<Customer>(query, pageIndex, pageSize);
            return customers;
        }

        /// <summary>
        /// Delete a customer tag
        /// </summary>
        /// <param name="customerTag">Customer tag</param>
        public virtual void DeleteCustomerTag(CustomerTag customerTag)
        {
            if (customerTag == null)
                throw new ArgumentNullException("productTag");

            var builder = Builders<Customer>.Update;
            var updatefilter = builder.Pull(x => x.CustomerTags, customerTag.Id);
            var result = _customerRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;

            _customerTagRepository.Delete(customerTag);

            //event notification
            _eventPublisher.EntityDeleted(customerTag);
        }

        /// <summary>
        /// Gets all customer tags
        /// </summary>
        /// <returns>Customer tags</returns>
        public virtual IList<CustomerTag> GetAllCustomerTags()
        {
            var query = _customerTagRepository.Table;
            return query.ToList();
        }

        /// <summary>
        /// Gets customer tag
        /// </summary>
        /// <param name="customerTagId">Customer tag identifier</param>
        /// <returns>Customer tag</returns>
        public virtual CustomerTag GetCustomerTagById(string customerTagId)
        {
            return _customerTagRepository.GetById(customerTagId);
        }

        /// <summary>
        /// Gets customer tag by name
        /// </summary>
        /// <param name="name">Customer tag name</param>
        /// <returns>Customer tag</returns>
        public virtual CustomerTag GetCustomerTagByName(string name)
        {
            var query = from pt in _customerTagRepository.Table
                        where pt.Name == name
                        select pt;

            return query.FirstOrDefault(); 
        }

        /// <summary>
        /// Gets customer tags search by name
        /// </summary>
        /// <param name="name">Customer tags name</param>
        /// <returns>Customer tags</returns>
        public virtual IList<CustomerTag> GetCustomerTagsByName(string name)
        {
            var query = from pt in _customerTagRepository.Table
                        where pt.Name.ToLower().Contains(name.ToLower())
                        select pt;
            return query.ToList();
        }

        /// <summary>
        /// Inserts a customer tag
        /// </summary>
        /// <param name="customerTag">Customer tag</param>
        public virtual void InsertCustomerTag(CustomerTag customerTag)
        {
            if (customerTag == null)
                throw new ArgumentNullException("customerTag");

            _customerTagRepository.Insert(customerTag);

            //event notification
            _eventPublisher.EntityInserted(customerTag);
        }

        /// <summary>
        /// Insert tag to a customer
        /// </summary>
        public virtual void InsertTagToCustomer(string customerTagId, string customerId)
        {
            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.AddToSet(p => p.CustomerTags, customerTagId);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("_id", customerId), update);
        }

        /// <summary>
        /// Delete tag from a customer
        /// </summary>
        public virtual void DeleteTagFromCustomer(string customerTagId, string customerId)
        {
            var updatebuilder = Builders<Customer>.Update;
            var update = updatebuilder.Pull(p => p.CustomerTags, customerTagId);
            _customerRepository.Collection.UpdateOneAsync(new BsonDocument("_id", customerId), update);
        }

        /// <summary>
        /// Updates the customer tag
        /// </summary>
        /// <param name="customerTag">Customer tag</param>
        public virtual void UpdateCustomerTag(CustomerTag customerTag)
        {
            if (customerTag == null)
                throw new ArgumentNullException("customerTag");

            _customerTagRepository.Update(customerTag);

            //event notification
            _eventPublisher.EntityUpdated(customerTag);
        }

        /// <summary>
        /// Get number of customers
        /// </summary>
        /// <param name="customerTagId">Customer tag identifier</param>
        /// <returns>Number of customers</returns>
        public virtual int GetCustomerCount(string customerTagId)
        {
            var query = _customerRepository.Table.Where(x => x.CustomerTags.Contains(customerTagId)).GroupBy(p => p, (k, s) => new { Count = s.Count() }).ToList();
            if (query.Count > 0)
                return query.FirstOrDefault().Count;
            return 0;
        }

    }
}
