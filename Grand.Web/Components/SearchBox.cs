using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class SearchBoxViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;

        public SearchBoxViewComponent(ICatalogViewModelService catalogViewModelService)
        {
            this._catalogViewModelService = catalogViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogViewModelService.PrepareSearchBox();
            return View(model);
        }
    }
}