using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
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
