using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Localization;
using iTextSharp.text.pdf;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class PreparePdfProductCommand : IRequest<PdfPTable>
    {
        public Product Product { get; set; }
        public Language Language { get; set; }
        public int ProductNumber { get; set; }
    }
}
