using Grand.Core.Domain.Vendors;
using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class VendorNavigationViewComponent : BaseViewComponent
    {
        private readonly ICatalogViewModelService _catalogViewModelService;
        private readonly VendorSettings _vendorSettings;
        public VendorNavigationViewComponent(ICatalogViewModelService catalogViewModelService,
            VendorSettings vendorSettings)
        {
            this._catalogViewModelService = catalogViewModelService;
            this._vendorSettings = vendorSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return Content("");

            var model = _catalogViewModelService.PrepareVendorNavigation();
            if (!model.Vendors.Any())
                return Content("");

            return View(model);
        }
    }
}