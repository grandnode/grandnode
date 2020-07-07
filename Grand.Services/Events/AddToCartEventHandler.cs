using Grand.Services.Customers;
using Grand.Services.Notifications.ShoppingCart;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public class AddToCartEventHandler : INotificationHandler<AddToCartEvent>
    {
        private readonly ICustomerActionEventService _customerActionEventService;

        public AddToCartEventHandler(ICustomerActionEventService customerActionEventService)
        {
            _customerActionEventService = customerActionEventService;
        }

        public async Task Handle(AddToCartEvent notification, CancellationToken cancellationToken)
        {
            await _customerActionEventService.AddToCart(notification.ShoppingCartItem, notification.Product, notification.Customer);
        }
    }
}
