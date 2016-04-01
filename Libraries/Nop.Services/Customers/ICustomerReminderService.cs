
using Nop.Core.Domain.Customers;
using System.Collections.Generic;

namespace Nop.Services.Customers
{
    public partial interface ICustomerReminderService
    {


        /// <summary>
        /// Gets customer reminder
        /// </summary>
        /// <param name="id">Customer reminder identifier</param>
        /// <returns>Customer reminder</returns>
        CustomerReminder GetCustomerReminderById(int id);


        /// <summary>
        /// Gets all customer reminders
        /// </summary>
        /// <returns>Customer reminders</returns>
        IList<CustomerReminder> GetCustomerReminders();

        /// <summary>
        /// Inserts a customer reminder
        /// </summary>
        /// <param name="CustomerReminder">Customer reminder</param>
        void InsertCustomerReminder(CustomerReminder customerReminder);

        /// <summary>
        /// Delete a customer reminder
        /// </summary>
        /// <param name="customerReminder">Customer reminder</param>
        void DeleteCustomerReminder(CustomerReminder customerReminder);

        /// <summary>
        /// Updates the customer reminder
        /// </summary>
        /// <param name="CustomerReminder">Customer reminder</param>
        void UpdateCustomerReminder(CustomerReminder customerReminder);

        /// <summary>
        /// Get allowed tokens for rule
        /// </summary>
        /// <param name="Rule">Customer Reminder Rule</param>
        string[] AllowedTokens(CustomerReminderRuleEnum rule);

    }
}
