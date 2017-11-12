using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.Vendors;

namespace Grand.Web.ViewComponents
{
    public class VendorNavigationViewComponent : ViewComponent
    {
        private readonly ICatalogWebService _catalogWebService;
        private readonly VendorSettings _vendorSettings;
        public VendorNavigationViewComponent(ICatalogWebService catalogWebService,
            VendorSettings vendorSettings)
        {
            this._catalogWebService = catalogWebService;
            this._vendorSettings = vendorSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return Content("");

            var model = _catalogWebService.PrepareVendorNavigation();
            if (!model.Vendors.Any())
                return Content("");

            return View(model);
        }
    }
}