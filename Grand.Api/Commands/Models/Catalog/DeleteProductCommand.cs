using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteProductCommand : IRequest<bool>
    {
        public ProductDto Model { get; set; }
    }
}
