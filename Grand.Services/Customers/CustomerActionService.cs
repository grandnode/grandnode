using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Services.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Grand.Core;
using Grand.Core.Caching;

namespace Grand.Services.Customers
{
    public partial class CustomerActionService: ICustomerActionService
    {
        #region Fields
        private const string CUSTOMER_ACTION_TYPE = "Grand.customer.action.type";
        private readonly IRepository<CustomerAction> _customerActionRepository;
        private readonly IRepository<CustomerActionType> _customerActionTypeRepository;
        private readonly IRepository<CustomerActionHistory> _customerActionHistoryRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public CustomerActionService(IRepository<CustomerAction> customerActionRepository,
            IRepository<CustomerActionType> customerActionTypeRepository,
            IRepository<CustomerActionHistory> customerActionHistoryRepository,
            IEventPublisher eventPublisher,
            ICacheManager cacheManager)
        {
            this._customerActionRepository = customerActionRepository;
            this._customerActionTypeRepository = customerActionTypeRepository;
            this._customerActionHistoryRepository = customerActionHistoryRepository;
            this._eventPublisher = eventPublisher;
            this._cacheManager = cacheManager;
        }

        #endregion


        #region Methods
        
        /// <summary>
        /// Gets customer action
        /// </summary>
        /// <param name="id">Customer action identifier</param>
        /// <returns>Customer Action</returns>
        public virtual CustomerAction GetCustomerActionById(string id)
        {
            return _customerActionRepository.GetById(id);
        }


        /// <summary>
        /// Gets all customer actions
        /// </summary>
        /// <returns>Customer actions</returns>
        public virtual IList<CustomerAction> GetCustomerActions()
        {
            var query = _customerActionRepository.Table;
            return query.ToList();
        }

        /// <summary>
        /// Inserts a customer action
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        public virtual void InsertCustomerAction(CustomerAction customerAction)
        {
            if (customerAction == null)
                throw new ArgumentNullException("customerAction");

            _customerActionRepository.Insert(customerAction);

            //event notification
            _eventPublisher.EntityInserted(customerAction);

        }

        /// <summary>
        /// Delete a customer action
        /// </summary>
        /// <param name="customerAction">Customer action</param>
        public virtual void DeleteCustomerAction(CustomerAction customerAction)
        {
            if (customerAction == null)
                throw new ArgumentNullException("customerAction"); 

            _customerActionRepository.Delete(customerAction);

            //event notification
            _eventPublisher.EntityDeleted(customerAction);

        }

        /// <summary>
        /// Updates the customer action
        /// </summary>
        /// <param name="customerTag">Customer tag</param>
        public virtual void UpdateCustomerAction(CustomerAction customerAction)
        {
            if (customerAction == null)
                throw new ArgumentNullException("customerAction");

            _customerActionRepository.Update(customerAction);

            //event notification
            _eventPublisher.EntityUpdated(customerAction);
        }

        #endregion

        #region Condition Type

        public virtual IList<CustomerActionType> GetCustomerActionType()
        {
            var query = _customerActionTypeRepository.Table;
            return query.ToList();
        }

        public virtual IPagedList<CustomerActionHistory> GetAllCustomerActionHistory(string customerActionId, int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = from h in _customerActionHistoryRepository.Table
                        where h.CustomerActionId == customerActionId
                        select h;
            var history = new PagedList<CustomerActionHistory>(query, pageIndex, pageSize);
            return history;
        }

        public virtual CustomerActionType GetCustomerActionTypeById(string id)
        {
            return _customerActionTypeRepository.GetById(id);
        }

        public virtual void UpdateCustomerActionType(CustomerActionType customerActionType)
        {
            if (customerActionType == null)
                throw new ArgumentNullException("customerActionType");

            _customerActionTypeRepository.Update(customerActionType);

            //clear cache
            _cacheManager.Remove(CUSTOMER_ACTION_TYPE);
            //event notification
            _eventPublisher.EntityUpdated(customerActionType);
        }

        #endregion

    }
}
