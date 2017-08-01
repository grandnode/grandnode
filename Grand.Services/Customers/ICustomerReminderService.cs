
using Grand.Core;
using Grand.Core.Domain.Customers;
using System.Collections.Generic;

namespace Grand.Services.Customers
{
    public partial interface ICustomerReminderService
    {


        /// <summary>
        /// Gets customer reminder
        /// </summary>
        /// <param name="id">Customer reminder identifier</param>
        /// <returns>Customer reminder</returns>
        CustomerReminder GetCustomerReminderById(string id);


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
        /// Gets customer reminders history for reminder
        /// </summary>
        /// <returns>SerializeCustomerReminderHistory</returns>
        IPagedList<SerializeCustomerReminderHistory> GetAllCustomerReminderHistory(string customerReminderId, int pageIndex = 0, int pageSize = 2147483647);

        /// <summary>
        /// Run task Abandoned Cart
        /// </summary>
        void Task_AbandonedCart(string id = "");
        void Task_RegisteredCustomer(string id = "");
        void Task_LastActivity(string id = "");
        void Task_LastPurchase(string id = "");
        void Task_Birthday(string id = "");
        void Task_CompletedOrder(string id = "");
        void Task_UnpaidOrder(string id = "");

    }
}
