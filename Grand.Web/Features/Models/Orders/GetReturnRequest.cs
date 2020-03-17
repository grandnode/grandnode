using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Stores;
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
