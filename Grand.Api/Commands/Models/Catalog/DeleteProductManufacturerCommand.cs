using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteProductManufacturerCommand : IRequest<bool>
    {
        public ProductDto Product { get; set; }
        public string ManufacturerId { get; set; }
    }
}
