using Grand.Domain.Shipping;
using MediatR;

namespace Grand.Services.Commands.Models.Shipping
{
    public class ShipCommand : IRequest<bool>
    {
        public Shipment Shipment { get; set; }
        public bool NotifyCustomer { get; set; }
    }
}
