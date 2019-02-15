using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class CategoryNavigationViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;

        public CategoryNavigationViewComponent(ICatalogViewModelService catalogViewModelService)
        {
            this._catalogViewModelService = catalogViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string currentCategoryId, string currentProductId)
        {
            var model = await Task.Run(() => _catalogViewModelService.PrepareCategoryNavigation(currentCategoryId, currentProductId));
            return View(model);
        }
    }
}