using AutoMapper;
using Grand.Core.Infrastructure.Mapper;
using Grand.Services.Shipping;
using Grand.Web.Areas.Admin.Models.Shipping;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class IShippingRateComputationMethodProfile : Profile, IMapperProfile
    {
        public IShippingRateComputationMethodProfile()
        {
            CreateMap<IShippingRateComputationMethod, ShippingRateComputationMethodModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                .ForMember(dest => dest.LogoUrl, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}