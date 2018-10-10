using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class HomePageManufacturersViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;

        public HomePageManufacturersViewComponent(ICatalogViewModelService catalogViewModelService)
        {
            this._catalogViewModelService = catalogViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogViewModelService.PrepareHomepageManufacturers();
            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}