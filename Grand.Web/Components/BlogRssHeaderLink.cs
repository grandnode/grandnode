using Grand.Core;
using Grand.Core.Domain.Blogs;
using Grand.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class BlogRssHeaderLinkViewComponent : BaseViewComponent
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

            return View();

        }
    }
}