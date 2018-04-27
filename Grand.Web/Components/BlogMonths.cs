using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Core.Domain.Blogs;

namespace Grand.Web.ViewComponents
{
    public class BlogMonthsViewComponent : ViewComponent
    {
        private readonly IBlogWebService _blogWebService;
        private readonly BlogSettings _blogSettings;

        public BlogMonthsViewComponent(IBlogWebService blogWebService, BlogSettings blogSettings)
        {
            this._blogWebService = blogWebService;
            this._blogSettings = blogSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var model = _blogWebService.PrepareBlogPostYearModel();
            return View(model);
        }
    }
}