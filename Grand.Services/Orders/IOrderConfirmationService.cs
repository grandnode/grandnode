using Grand.Domain.Orders;
using Grand.Services.Payments;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    public interface IOrderConfirmationService
    {
        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        Task<PlaceOrderResult> PlaceOrder(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Send notification order 
        /// </summary>
        /// <param name="order">Order</param>
        Task SendNotification(Order order);

    }
}
