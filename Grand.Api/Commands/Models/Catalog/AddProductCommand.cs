using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddProductCommand : IRequest<ProductDto>
    {
        public ProductDto Model { get; set; }
    }
}
