using Grand.Web.Models.Order;
using MediatR;

namespace Grand.Web.Features.Models.Orders
{
    public class GetShipmentDetails : IRequest<ShipmentDetailsModel>
    {
        public Core.Domain.Shipping.Shipment Shipment { get; set; }
        public Core.Domain.Orders.Order Order { get; set; }
        public Core.Domain.Customers.Customer Customer { get; set; }
        public Core.Domain.Localization.Language Language { get; set; }
    }
}
