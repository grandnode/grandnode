using AutoMapper;
using Grand.Domain.Messages;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Messages;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class BannerProfile : Profile, IMapperProfile
    {
        public BannerProfile()
        {
            CreateMap<Banner, BannerModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<BannerModel, Banner>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}