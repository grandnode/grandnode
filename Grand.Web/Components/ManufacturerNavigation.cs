using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.Catalog;

namespace Grand.Web.ViewComponents
{
    public class ManufacturerNavigationViewComponent : ViewComponent
    {
        private readonly ICatalogWebService _catalogWebService;
        private readonly CatalogSettings _catalogSettings;

        public ManufacturerNavigationViewComponent(ICatalogWebService catalogWebService,
            CatalogSettings catalogSettings)
        {
            this._catalogWebService = catalogWebService;
            this._catalogSettings = catalogSettings;
        }

        public IViewComponentResult Invoke(string currentManufacturerId)
        {
            if (_catalogSettings.ManufacturersBlockItemsToDisplay == 0)
                return Content("");

            var model = _catalogWebService.PrepareManufacturerNavigation(currentManufacturerId);
            if (!model.Manufacturers.Any())
                return Content("");

            return View(model);
        }
    }
}