using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Orders;
using MediatR;

namespace Grand.Web.Features.Models.Orders
{
    public class GetReturnRequest : IRequest<ReturnRequestModel>
    {
        public Order Order { get; set; }
    }
}
