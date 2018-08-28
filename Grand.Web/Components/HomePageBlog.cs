using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Core.Domain.Blogs;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class HomePageBlogViewComponent : BaseViewComponent
    {
        private readonly IBlogWebService _blogWebService;
        private readonly BlogSettings _blogSettings;
        public HomePageBlogViewComponent(IBlogWebService blogWebService,
            BlogSettings blogSettings)
        {
            this._blogWebService = blogWebService;
            this._blogSettings = blogSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_blogSettings.Enabled || !_blogSettings.ShowBlogOnHomePage)
                return Content("");

            var model = _blogWebService.PrepareHomePageBlogItems();
            return View(model);
        }
    }
}