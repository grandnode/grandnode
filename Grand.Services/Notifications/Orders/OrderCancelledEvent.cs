using Grand.Domain.Orders;
using MediatR;

namespace Grand.Services.Notifications.Orders
{
    /// <summary>
    /// Order cancelled event
    /// </summary>
    public class OrderCancelledEvent : INotification
    {
        public OrderCancelledEvent(Order order)
        {
            Order = order;
        }

        /// <summary>
        /// Order
        /// </summary>
        public Order Order { get; private set; }
    }
}
