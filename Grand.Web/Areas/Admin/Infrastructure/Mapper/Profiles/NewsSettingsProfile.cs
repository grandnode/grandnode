using AutoMapper;
using Grand.Domain.News;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class NewsSettingsProfile : Profile, IMapperProfile
    {
        public NewsSettingsProfile()
        {
            CreateMap<NewsSettings, NewsSettingsModel>()
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NotifyAboutNewNewsComments_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowNewsOnMainPage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MainPageNewsCount_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NewsArchivePageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowHeaderRssUrl_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());
            CreateMap<NewsSettingsModel, NewsSettings>();
        }

        public int Order => 0;
    }
}