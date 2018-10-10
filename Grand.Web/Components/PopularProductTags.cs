using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class PopularProductTagsViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;

        public PopularProductTagsViewComponent(ICatalogViewModelService catalogViewModelService)
        {
            this._catalogViewModelService = catalogViewModelService;
        }

        public IViewComponentResult Invoke(string currentCategoryId, string currentProductId)
        {
            var model = _catalogViewModelService.PreparePopularProductTags();
            if (!model.Tags.Any())
                return Content("");

            return View(model);

        }
    }
}