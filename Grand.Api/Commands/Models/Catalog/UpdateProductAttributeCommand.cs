using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateProductAttributeCommand : IRequest<ProductAttributeDto>
    {
        public ProductAttributeDto Model { get; set; }
    }
}
