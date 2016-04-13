using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace Nop.Services.Customers
{
    public partial class CustomerActionService: ICustomerActionService
    {
        #region Fields

        private readonly IRepository<CustomerAction> _customerActionRepository;
        private readonly IRepository<CustomerActionType> _customerActionTypeRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public CustomerActionService(IRepository<CustomerAction> customerActionRepository,
            IRepository<CustomerActionType> customerActionTypeRepository,
            IEventPublisher eventPublisher)
        {
            this._customerActionRepository = customerActionRepository;
            this._customerActionTypeRepository = customerActionTypeRepository;
            this._eventPublisher = eventPublisher;
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
        public virtual CustomerActionType GetCustomerActionTypeById(string id)
        {
            return _customerActionTypeRepository.GetById(id);
        }

        public virtual void UpdateCustomerActionType(CustomerActionType customerActionType)
        {
            if (customerActionType == null)
                throw new ArgumentNullException("customerActionType");

            _customerActionTypeRepository.Update(customerActionType);

            //event notification
            _eventPublisher.EntityUpdated(customerActionType);
        }

        #endregion

    }
}
