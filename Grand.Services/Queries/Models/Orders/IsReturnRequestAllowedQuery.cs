using Grand.Domain.Orders;
using MediatR;

namespace Grand.Services.Queries.Models.Orders
{
    public class IsReturnRequestAllowedQuery : IRequest<bool>
    {
        public Order Order { get; set; }
    }
}
