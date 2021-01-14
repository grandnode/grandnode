using AutoMapper;
using Grand.Domain.Shipping;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Shipping;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class ShippingMethodProfile : Profile, IMapperProfile
    {
        public ShippingMethodProfile()
        {
            CreateMap<ShippingMethod, ShippingMethodModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<ShippingMethodModel, ShippingMethod>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.RestrictedCountries, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}