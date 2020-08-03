using AutoMapper;
using Grand.Api.DTOs.Common;
using Grand.Domain.Media;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class PictureProfile : Profile, IMapperProfile
    {
        public PictureProfile()
        {
            CreateMap<PictureDto, Picture>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<Picture, PictureDto>()
                .ForMember(dest => dest.PictureBinary, mo => mo.Ignore());
        }

        public int Order => 1;
    }
}