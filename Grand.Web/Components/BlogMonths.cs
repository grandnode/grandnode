using Grand.Core.Domain.Blogs;
using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class BlogMonthsViewComponent : BaseViewComponent
    {
        private readonly IBlogViewModelService _blogViewModelService;
        private readonly BlogSettings _blogSettings;

        public BlogMonthsViewComponent(IBlogViewModelService blogViewModelService, BlogSettings blogSettings)
        {
            this._blogViewModelService = blogViewModelService;
            this._blogSettings = blogSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var model = _blogViewModelService.PrepareBlogPostYearModel();
            return View(model);
        }
    }
}