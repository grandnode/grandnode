using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using MediatR;
namespace Grand.Web.Features.Models.Catalog
{
    public class GetFormatBasePrice : IRequest<string>
    {
        public Currency Currency { get; set; }
        public Product Product { get; set; }
        public decimal? ProductPrice { get; set; }
    }
}
