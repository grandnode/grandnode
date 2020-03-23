using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Services.Customers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public class OrderPaidEventHandler : INotificationHandler<OrderPaidEvent>
    {
        private readonly ICustomerActionEventService _customerActionEventService;

        public OrderPaidEventHandler(ICustomerActionEventService customerActionEventService)
        {
            _customerActionEventService = customerActionEventService;
        }

        public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
        {
            //customer action event service - paid order
            await _customerActionEventService.AddOrder(notification.Order, CustomerActionTypeEnum.PaidOrder);
        }
    }
}
