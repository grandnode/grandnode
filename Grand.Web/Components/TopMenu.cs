using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class TopMenuViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;

        public TopMenuViewComponent(ICatalogViewModelService catalogViewModelService)
        {
            this._catalogViewModelService = catalogViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogViewModelService.PrepareTopMenu();
            return View(model);

        }
    }
}