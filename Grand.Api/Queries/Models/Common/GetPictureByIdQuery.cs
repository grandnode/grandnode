using Grand.Api.DTOs.Common;
using MediatR;

namespace Grand.Api.Queries.Models.Common
{
    public class GetPictureByIdQuery : IRequest<PictureDto>
    {
        public string Id { get; set; }
    }
}
