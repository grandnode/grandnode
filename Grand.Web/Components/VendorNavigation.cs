using Grand.Core.Domain.Vendors;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class VendorNavigationViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;
        private readonly VendorSettings _vendorSettings;
        public VendorNavigationViewComponent(ICatalogViewModelService catalogViewModelService,
            VendorSettings vendorSettings)
        {
            _catalogViewModelService = catalogViewModelService;
            _vendorSettings = vendorSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return Content("");

            var model = await _catalogViewModelService.PrepareVendorNavigation();
            if (!model.Vendors.Any())
                return Content("");

            return View(model);
        }
    }
}