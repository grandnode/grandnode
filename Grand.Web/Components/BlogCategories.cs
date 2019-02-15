using Grand.Core.Domain.Blogs;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class BlogCategoriesViewComponent : BaseViewComponent
    {
        private readonly IBlogViewModelService _blogViewModelService;
        private readonly BlogSettings _blogSettings;

        public BlogCategoriesViewComponent(IBlogViewModelService blogViewModelService, BlogSettings blogSettings)
        {
            this._blogViewModelService = blogViewModelService;
            this._blogSettings = blogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var model = await Task.Run(() => _blogViewModelService.PrepareBlogPostCategoryModel());
            return View(model);
        }
    }
}