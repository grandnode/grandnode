using AutoMapper;
using Grand.Domain.Blogs;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class BlogSettingsProfile : Profile, IMapperProfile
    {
        public BlogSettingsProfile()
        {
            CreateMap<BlogSettings, BlogSettingsModel>()
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PostsPageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NotifyAboutNewBlogComments_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfTags_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowHeaderRssUrl_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());
            CreateMap<BlogSettingsModel, BlogSettings>();
        }

        public int Order => 0;
    }
}