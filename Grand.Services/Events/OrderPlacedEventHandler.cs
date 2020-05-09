using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Services.Customers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public class OrderPlacedEventHandler : INotificationHandler<OrderPlacedEvent>
    {
        private readonly ICustomerActionEventService _customerActionEventService;

        public OrderPlacedEventHandler(ICustomerActionEventService customerActionEventService)
        {
            _customerActionEventService = customerActionEventService;
        }

        public async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
        {
            //cutomer action - add order
            await _customerActionEventService.AddOrder(notification.Order, CustomerActionTypeEnum.AddOrder);
        }
    }
}
