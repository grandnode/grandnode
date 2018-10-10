using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class CategoryNavigationViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;

        public CategoryNavigationViewComponent(ICatalogViewModelService catalogViewModelService)
        {
            this._catalogViewModelService = catalogViewModelService;
        }

        public IViewComponentResult Invoke(string currentCategoryId, string currentProductId)
        {
            var model = _catalogViewModelService.PrepareCategoryNavigation(currentCategoryId, currentProductId);
            return View(model);
        }
    }
}