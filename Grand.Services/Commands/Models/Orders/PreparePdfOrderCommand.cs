using Grand.Core.Domain.Orders;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class PreparePdfOrderCommand : IRequest<bool>
    {
        public Document Doc { get; set; }
        public PdfWriter PdfWriter { get; set; }
        public Rectangle PageSize { get; set; }
        public Order Order { get; set; }
        public string LanguageId { get; set; } = "";
        public string VendorId { get; set; } = "";
    }
}
