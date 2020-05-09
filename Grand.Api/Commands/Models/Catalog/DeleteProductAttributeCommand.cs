using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteProductAttributeCommand : IRequest<bool>
    {
        public ProductAttributeDto Model { get; set; }
    }
}
