using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateSpecificationAttributeCommand : IRequest<SpecificationAttributeDto>
    {
        public SpecificationAttributeDto Model { get; set; }
    }
}
