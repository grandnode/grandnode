using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Framework.Components;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ShoppingCartLinksViewComponent : BaseViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public ShoppingCartLinksViewComponent(
            IWorkContext workContext,
            IStoreContext storeContext,
            IPermissionService permissionService,
            ShoppingCartSettings shoppingCartSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _permissionService = permissionService;
            _shoppingCartSettings = shoppingCartSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await PrepareShoppingCartLinks();
            return View(model);
        }

        private async Task<ShoppingCartLinksModel> PrepareShoppingCartLinks()
        {
            var model = new ShoppingCartLinksModel {
                ShoppingCartEnabled = await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                WishlistEnabled = await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                MiniShoppingCartEnabled = _shoppingCartSettings.MiniShoppingCartEnabled,
                //performance optimization (use "HasShoppingCartItems" property)
                ShoppingCartItems = _workContext.CurrentCustomer.ShoppingCartItems.Any() ? _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                        .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, _storeContext.CurrentStore.Id)
                        .Sum(x => x.Quantity) : 0,

                WishlistItems = _workContext.CurrentCustomer.ShoppingCartItems.Any() ? _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                        .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, _storeContext.CurrentStore.Id)
                        .Sum(x => x.Quantity) : 0
            };
            return model;
        }

    }
}