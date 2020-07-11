using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Stores;
using Grand.Web.Models.Orders;
using MediatR;

namespace Grand.Web.Features.Models.Orders
{
    public class GetReturnRequest : IRequest<ReturnRequestModel>
    {
        public Order Order { get; set; }
        public Language Language { get; set; }
        public Store Store { get; set; }
    }
}
