using AutoMapper;
using Grand.Core.Mapper;
using Grand.Services.Cms;
using Grand.Web.Areas.Admin.Models.Cms;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class IWidgetPluginProfile : Profile, IAutoMapperProfile
    {
        public IWidgetPluginProfile()
        {
            CreateMap<IWidgetPlugin, WidgetModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                .ForMember(dest => dest.IsActive, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}