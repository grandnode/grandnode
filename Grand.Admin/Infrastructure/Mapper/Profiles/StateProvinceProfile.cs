using AutoMapper;
using Grand.Domain.Directory;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Directory;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class StateProvinceProfile : Profile, IMapperProfile
    {
        public StateProvinceProfile()
        {
            CreateMap<StateProvince, StateProvinceModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            CreateMap<StateProvinceModel, StateProvince>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}