using Grand.Domain.Orders;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class CancelOrderCommand : IRequest<bool>
    {
        public Order Order { get; set; }
        public bool NotifyCustomer { get; set; }
        public bool NotifyStoreOwner { get; set; } = false;
    }
}
