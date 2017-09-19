using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;

namespace Grand.Web.ViewComponents
{
    public class CategoryNavigationViewComponent : ViewComponent
    {
        private readonly ICatalogWebService _catalogWebService;

        public CategoryNavigationViewComponent(ICatalogWebService catalogWebService)
        {
            this._catalogWebService = catalogWebService;
        }

        public IViewComponentResult Invoke(string currentCategoryId, string currentProductId)
        {
            var model = _catalogWebService.PrepareCategoryNavigation(currentCategoryId, currentProductId);
            return View(model);
        }
    }
}