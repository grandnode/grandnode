using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddManufacturerCommand : IRequest<ManufacturerDto>
    {
        public ManufacturerDto Model { get; set; }
    }
}
