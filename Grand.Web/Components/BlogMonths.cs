using Grand.Core.Domain.Blogs;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class BlogMonthsViewComponent : BaseViewComponent
    {
        private readonly IBlogViewModelService _blogViewModelService;
        private readonly BlogSettings _blogSettings;

        public BlogMonthsViewComponent(IBlogViewModelService blogViewModelService, BlogSettings blogSettings)
        {
            _blogViewModelService = blogViewModelService;
            _blogSettings = blogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var model = await _blogViewModelService.PrepareBlogPostYearModel();
            return View(model);
        }
    }
}