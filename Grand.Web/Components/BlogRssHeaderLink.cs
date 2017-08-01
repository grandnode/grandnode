using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.News;
using Grand.Core;
using Grand.Services.Localization;
using Grand.Core.Domain.Blogs;

namespace Grand.Web.ViewComponents
{
    public class BlogRssHeaderLinkViewComponent : ViewComponent
    {
        private readonly BlogSettings _blogSettings;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        public BlogRssHeaderLinkViewComponent(BlogSettings blogSettings, IWorkContext workContext, IWebHelper webHelper, IStoreContext storeContext)
        {
            this._blogSettings = blogSettings;
            this._workContext = workContext;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            if (!_blogSettings.Enabled || !_blogSettings.ShowHeaderRssUrl)
                return Content("");

            string link = string.Format("<link href=\"{0}\" rel=\"alternate\" type=\"application/rss+xml\" title=\"{1}: Blog\" />",
                Url.RouteUrl("BlogRSS", new { languageId = _workContext.WorkingLanguage.Id }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http"), _storeContext.CurrentStore.GetLocalized(x => x.Name));

            return Content(link);

        }
    }
}