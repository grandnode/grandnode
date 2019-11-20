using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ManufacturerNavigationViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;
        private readonly CatalogSettings _catalogSettings;

        public ManufacturerNavigationViewComponent(ICatalogViewModelService catalogViewModelService,
            CatalogSettings catalogSettings)
        {
            _catalogViewModelService = catalogViewModelService;
            _catalogSettings = catalogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string currentManufacturerId)
        {
            if (_catalogSettings.ManufacturersBlockItemsToDisplay == 0)
                return Content("");

            var model = await _catalogViewModelService.PrepareManufacturerNavigation(currentManufacturerId);
            if (!model.Manufacturers.Any())
                return Content("");

            return View(model);
        }
    }
}