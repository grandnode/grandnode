using Grand.Core.Domain.Blogs;
using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class HomePageBlogViewComponent : BaseViewComponent
    {
        private readonly IBlogViewModelService _blogViewModelService;
        private readonly BlogSettings _blogSettings;
        public HomePageBlogViewComponent(IBlogViewModelService blogViewModelService,
            BlogSettings blogSettings)
        {
            this._blogViewModelService = blogViewModelService;
            this._blogSettings = blogSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_blogSettings.Enabled || !_blogSettings.ShowBlogOnHomePage)
                return Content("");

            var model = _blogViewModelService.PrepareHomePageBlogItems();
            return View(model);
        }
    }
}