using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Framework.Components;
using Grand.Services.Orders;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class EstimateShippingViewComponent : BaseViewComponent
    {
        private readonly IShoppingCartViewModelService _shoppingCartViewModelService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        public EstimateShippingViewComponent(IShoppingCartViewModelService shoppingCartViewModelService, IShoppingCartService shoppingCartService, IStoreContext storeContext)
        {
            _shoppingCartViewModelService = shoppingCartViewModelService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool? prepareAndDisplayOrderReviewData)
        {
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart);

            var model = await _shoppingCartViewModelService.PrepareEstimateShipping(cart);
            if (!model.Enabled)
                return Content("");

            return View(model);
        }

    }
}
