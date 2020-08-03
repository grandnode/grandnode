using AutoMapper;
using Grand.Core.Infrastructure.Mapper;
using Grand.Services.Authentication.External;
using Grand.Web.Areas.Admin.Models.ExternalAuthentication;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class IExternalAuthenticationMethodProfile : Profile, IMapperProfile
    {
        public IExternalAuthenticationMethodProfile()
        {
            CreateMap<IExternalAuthenticationMethod, AuthenticationMethodModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                .ForMember(dest => dest.IsActive, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}