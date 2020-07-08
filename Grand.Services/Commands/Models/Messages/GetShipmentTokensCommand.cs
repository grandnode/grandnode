using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;

namespace Grand.Services.Commands.Models.Messages
{
    public class GetShipmentTokensCommand : IRequest<LiquidShipment>
    {
        public Shipment Shipment { get; set; }
        public Order Order { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
    }
}
