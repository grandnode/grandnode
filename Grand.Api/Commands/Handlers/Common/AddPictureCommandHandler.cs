using Grand.Api.Commands.Models.Common;
using Grand.Api.DTOs.Common;
using Grand.Api.Extensions;
using Grand.Services.Media;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Handlers.Common
{
    public class AddPictureCommandHandler : IRequestHandler<AddPictureCommand, PictureDto>
    {
        private readonly IPictureService _pictureService;

        public AddPictureCommandHandler(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        public async Task<PictureDto> Handle(AddPictureCommand request, CancellationToken cancellationToken)
        {
            var picture = await _pictureService.InsertPicture(request.PictureDto.PictureBinary, request.PictureDto.MimeType,
                request.PictureDto.SeoFilename,
                request.PictureDto.AltAttribute,
                request.PictureDto.TitleAttribute,
                request.PictureDto.IsNew);

            return picture.ToModel();

        }
    }
}
