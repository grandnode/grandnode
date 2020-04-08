using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddProductCategoryCommand : IRequest<bool>
    {
        public ProductDto Product { get; set; }
        public ProductCategoryDto Model { get; set; }
    }
}
