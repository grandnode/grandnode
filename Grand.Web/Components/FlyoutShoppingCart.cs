using Grand.Core.Domain.Orders;
using Grand.Framework.Components;
using Grand.Services.Security;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class FlyoutShoppingCartViewComponent : BaseViewComponent
    {
        private readonly IShoppingCartViewModelService _shoppingCartViewModelService;
        private readonly IPermissionService _permissionService;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public FlyoutShoppingCartViewComponent(IShoppingCartViewModelService shoppingCartViewModelService,
            IPermissionService permissionService,
            ShoppingCartSettings shoppingCartSettings)
        {
            _shoppingCartViewModelService = shoppingCartViewModelService;
            _permissionService = permissionService;
            _shoppingCartSettings = shoppingCartSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");

            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return Content("");

            var model = await _shoppingCartViewModelService.PrepareMiniShoppingCart();
            return View(model);
        }
    }
}