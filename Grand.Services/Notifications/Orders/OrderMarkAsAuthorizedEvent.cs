using Grand.Domain.Orders;
using MediatR;

namespace Grand.Services.Notifications.Orders
{
    /// <summary>
    /// Order mark as authorized event
    /// </summary>
    public class OrderMarkAsAuthorizedEvent : INotification
    {
        public OrderMarkAsAuthorizedEvent(Order order)
        {
            Order = order;
        }

        /// <summary>
        /// Order
        /// </summary>
        public Order Order { get; private set; }
    }

}
