using AutoMapper;
using Grand.Domain.Stores;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Stores;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class StoreProfile : Profile, IMapperProfile
    {
        public StoreProfile()
        {
            CreateMap<Store, StoreModel>()
                .ForMember(dest => dest.AvailableLanguages, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableWarehouses, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCountries, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCurrencies, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            CreateMap<StoreModel, Store>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}