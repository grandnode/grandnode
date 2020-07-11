using Grand.Domain.Customers;
using MediatR;

namespace Grand.Services.Notifications.Customers
{
    /// <summary>
    /// Customer registered event
    /// </summary>
    public class CustomerRegisteredEvent : INotification
    {
        public CustomerRegisteredEvent(Customer customer)
        {
            Customer = customer;
        }

        /// <summary>
        /// Customer
        /// </summary>
        public Customer Customer {
            get; private set;
        }
    }

}
