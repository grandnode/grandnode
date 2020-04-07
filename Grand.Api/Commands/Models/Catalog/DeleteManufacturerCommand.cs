using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteManufacturerCommand : IRequest<bool>
    {
        public ManufacturerDto Model { get; set; }
    }
}
