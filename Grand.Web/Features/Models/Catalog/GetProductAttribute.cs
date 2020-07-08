using Grand.Domain.Catalog;
using MediatR;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetProductAttribute : IRequest<ProductAttribute>
    {
        public string Id { get; set; }
    }
}
