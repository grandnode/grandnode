using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Web.Models.Orders;
using MediatR;

namespace Grand.Web.Features.Models.Orders
{
    public class GetShipmentDetails : IRequest<ShipmentDetailsModel>
    {
        public Shipment Shipment { get; set; }
        public Order Order { get; set; }
        public Customer Customer { get; set; }
        public Language Language { get; set; }
    }
}
