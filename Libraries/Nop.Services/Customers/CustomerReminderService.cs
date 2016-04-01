using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Nop.Services.Customers
{
    public partial class CustomerReminderService : ICustomerReminderService
    {
        #region Fields

        private readonly IRepository<CustomerReminder> _customerReminderRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public CustomerReminderService(IRepository<CustomerReminder> customerReminderRepository,
            IEventPublisher eventPublisher)
        {
            this._customerReminderRepository = customerReminderRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion


        #region Methods

        /// <summary>
        /// Gets customer reminder
        /// </summary>
        /// <param name="id">Customer reminder identifier</param>
        /// <returns>Customer reminder</returns>
        public virtual CustomerReminder GetCustomerReminderById(int id)
        {
            if (id == 0)
                return null;
            return _customerReminderRepository.GetById(id);
        }


        /// <summary>
        /// Gets all customer reminders
        /// </summary>
        /// <returns>Customer reminders</returns>
        public virtual IList<CustomerReminder> GetCustomerReminders()
        {
            var query = from p in _customerReminderRepository.Table
                        orderby p.DisplayOrder
                        select p;
            return query.ToList();
        }

        /// <summary>
        /// Inserts a customer reminder
        /// </summary>
        /// <param name="CustomerReminder">Customer reminder</param>
        public virtual void InsertCustomerReminder(CustomerReminder customerReminder)
        {
            if (customerReminder == null)
                throw new ArgumentNullException("customerReminder");

            _customerReminderRepository.Insert(customerReminder);

            //event notification
            _eventPublisher.EntityInserted(customerReminder);

        }

        /// <summary>
        /// Delete a customer reminder
        /// </summary>
        /// <param name="customerReminder">Customer reminder</param>
        public virtual void DeleteCustomerReminder(CustomerReminder customerReminder)
        {
            if (customerReminder == null)
                throw new ArgumentNullException("customerReminder");

            _customerReminderRepository.Delete(customerReminder);

            //event notification
            _eventPublisher.EntityDeleted(customerReminder);

        }

        /// <summary>
        /// Updates the customer reminder
        /// </summary>
        /// <param name="CustomerReminder">Customer reminder</param>
        public virtual void UpdateCustomerReminder(CustomerReminder customerReminder)
        {
            if (customerReminder == null)
                throw new ArgumentNullException("customerReminder");

            _customerReminderRepository.Update(customerReminder);

            //event notification
            _eventPublisher.EntityUpdated(customerReminder);
        }


        /// <summary>
        /// Get allowed tokens for rule
        /// </summary>
        /// <param name="Rule">Customer Reminder Rule</param>
        public string[] AllowedTokens(CustomerReminderRuleEnum rule)
        {
            var allowedTokens = new List<string>();
            allowedTokens.AddRange(
                new List<string>{ "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Store.CompanyName%",
                "%Store.CompanyAddress%",
                "%Store.CompanyPhoneNumber%",
                "%Store.CompanyVat%",
                "%Twitter.URL%",
                "%Facebook.URL%",
                "%YouTube.URL%",
                "%GooglePlus.URL%"}
                );

            if(rule == CustomerReminderRuleEnum.AbandonedCart)
            {
                allowedTokens.Add("%Cart%");

            }
            allowedTokens.AddRange(
                new List<string>{
                "%Customer.Email%",
                "%Customer.Username%",
                "%Customer.FullName%",
                "%Customer.FirstName%",
                "%Customer.LastName%"
                });
            return allowedTokens.ToArray();
        }


        #endregion


    }
}
