using Grand.Web.Models.Orders;
using MediatR;

namespace Grand.Web.Features.Models.Orders
{
    public class GetOrderDetails : IRequest<OrderDetailsModel>
    {
        public Domain.Orders.Order Order { get; set; }
        public Domain.Localization.Language Language { get; set; }
    }
}
