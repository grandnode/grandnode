using Grand.Domain.Orders;
using MediatR;

namespace Grand.Services.Notifications.Orders
{
    /// <summary>
    /// Order void offline event
    /// </summary>
    public class OrderVoidOfflineEvent : INotification
    {
        public OrderVoidOfflineEvent(Order order)
        {
            Order = order;
        }

        /// <summary>
        /// Order
        /// </summary>
        public Order Order { get; private set; }
    }
}
