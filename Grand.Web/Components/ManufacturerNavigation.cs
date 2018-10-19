using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class ManufacturerNavigationViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;
        private readonly CatalogSettings _catalogSettings;

        public ManufacturerNavigationViewComponent(ICatalogViewModelService catalogViewModelService,
            CatalogSettings catalogSettings)
        {
            this._catalogViewModelService = catalogViewModelService;
            this._catalogSettings = catalogSettings;
        }

        public IViewComponentResult Invoke(string currentManufacturerId)
        {
            if (_catalogSettings.ManufacturersBlockItemsToDisplay == 0)
                return Content("");

            var model = _catalogViewModelService.PrepareManufacturerNavigation(currentManufacturerId);
            if (!model.Manufacturers.Any())
                return Content("");

            return View(model);
        }
    }
}