using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteSpecificationAttributeCommand : IRequest<bool>
    {
        public SpecificationAttributeDto Model { get; set; }
    }
}
