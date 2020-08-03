using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public CategoryDto Model { get; set; }
    }
}
