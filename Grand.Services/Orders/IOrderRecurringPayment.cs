using Grand.Domain.Customers;
using Grand.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    public interface IOrderRecurringPayment
    {
        /// <summary>
        /// Process next recurring psayment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        Task ProcessNextRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        Task<IList<string>> CancelRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Gets a value indicating whether a customer can cancel recurring payment
        /// </summary>
        /// <param name="customerToValidate">Customer</param>
        /// <param name="recurringPayment">Recurring Payment</param>
        /// <returns>value indicating whether a customer can cancel recurring payment</returns>
        Task<bool> CanCancelRecurringPayment(Customer customerToValidate, RecurringPayment recurringPayment);
    }
}
