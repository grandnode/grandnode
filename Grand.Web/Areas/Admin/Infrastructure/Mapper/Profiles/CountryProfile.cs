using AutoMapper;
using Grand.Domain.Directory;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Directory;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class CountryProfile : Profile, IMapperProfile
    {
        public CountryProfile()
        {
            //countries
            CreateMap<CountryModel, Country>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.Stores, mo => mo.MapFrom(x => x.SelectedStoreIds != null ? x.SelectedStoreIds.ToList() : new List<string>()))
                .ForMember(dest => dest.StateProvinces, mo => mo.Ignore())
                .ForMember(dest => dest.RestrictedShippingMethods, mo => mo.Ignore());
            CreateMap<Country, CountryModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfStates, mo => mo.MapFrom(src => src.StateProvinces != null ? src.StateProvinces.Count : 0))
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}
