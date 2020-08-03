using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddProductPictureCommand : IRequest<bool>
    {
        public ProductDto Product { get; set; }
        public ProductPictureDto Model { get; set; }
    }
}
