using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class BlogSettingsModel : BaseGrandModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }



        [GrandResourceDisplayName("Admin.Configuration.Settings.Blog.Enabled")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Blog.PostsPageSize")]
        public int PostsPageSize { get; set; }
        public bool PostsPageSize_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Blog.AllowNotRegisteredUsersToLeaveComments")]
        public bool AllowNotRegisteredUsersToLeaveComments { get; set; }
        public bool AllowNotRegisteredUsersToLeaveComments_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Blog.NotifyAboutNewBlogComments")]
        public bool NotifyAboutNewBlogComments { get; set; }
        public bool NotifyAboutNewBlogComments_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Blog.NumberOfTags")]
        public int NumberOfTags { get; set; }
        public bool NumberOfTags_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Blog.ShowHeaderRSSUrl")]
        public bool ShowHeaderRssUrl { get; set; }
        public bool ShowHeaderRssUrl_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Blog.ShowBlogOnHomePage")]
        public bool ShowBlogOnHomePage { get; set; }
        public bool ShowBlogOnHomePage_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Blog.HomePageBlogCount")]
        public int HomePageBlogCount { get; set; }
        public bool HomePageBlogCount_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Blog.MaxTextSizeHomePage")]
        public int MaxTextSizeHomePage { get; set; }
        public bool MaxTextSizeHomePage_OverrideForStore { get; set; }
    }
}