using Grand.Api.DTOs.Common;
using MediatR;

namespace Grand.Api.Commands.Models.Common
{
    public class AddPictureCommand : IRequest<PictureDto>
    {
        public PictureDto PictureDto { get; set; }
    }
}
