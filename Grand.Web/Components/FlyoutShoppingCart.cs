using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Core.Domain.Orders;
using Grand.Services.Security;

namespace Grand.Web.ViewComponents
{
    public class FlyoutShoppingCartViewComponent : ViewComponent
    {
        private readonly IShoppingCartWebService _shoppingCartWebService;
        private readonly IPermissionService _permissionService;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public FlyoutShoppingCartViewComponent(IShoppingCartWebService shoppingCartWebService,
            IPermissionService permissionService,
            ShoppingCartSettings shoppingCartSettings)
        {
            this._shoppingCartWebService = shoppingCartWebService;
            this._permissionService = permissionService;
            this._shoppingCartSettings = shoppingCartSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return Content("");

            var model = _shoppingCartWebService.PrepareMiniShoppingCart();
            return View(model);
        }
    }
}