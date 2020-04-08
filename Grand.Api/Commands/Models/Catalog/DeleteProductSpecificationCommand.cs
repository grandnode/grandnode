using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteProductSpecificationCommand : IRequest<bool>
    {
        public ProductDto Product { get; set; }
        public string Id { get; set; }
    }
}
