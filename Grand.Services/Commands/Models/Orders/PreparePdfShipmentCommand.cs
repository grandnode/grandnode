using Grand.Domain.Localization;
using Grand.Domain.Shipping;
using iTextSharp.text;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class PreparePdfShipmentCommand : IRequest<bool>
    {
        public Document Doc { get; set; }
        public Shipment Shipment { get; set; }
        public string LanguageId { get; set; } = "";
        public Language Language { get; set; }
    }
}
