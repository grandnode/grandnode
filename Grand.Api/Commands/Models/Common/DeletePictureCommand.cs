using Grand.Api.DTOs.Common;
using MediatR;

namespace Grand.Api.Commands.Models.Common
{
    public class DeletePictureCommand : IRequest<bool>
    {
        public PictureDto PictureDto { get; set; }
    }
}
