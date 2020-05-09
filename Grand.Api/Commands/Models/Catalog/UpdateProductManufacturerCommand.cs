using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateProductManufacturerCommand : IRequest<bool>
    {
        public ProductDto Product { get; set; }
        public ProductManufacturerDto Model { get; set; }
    }
}
