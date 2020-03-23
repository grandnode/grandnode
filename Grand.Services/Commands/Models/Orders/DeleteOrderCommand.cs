using Grand.Core.Domain.Orders;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class DeleteOrderCommand : IRequest<bool>
    {
        public Order Order { get; set; }
    }
}
