using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddCategoryCommand : IRequest<CategoryDto>
    {
        public CategoryDto Model { get; set; }
    }
}
