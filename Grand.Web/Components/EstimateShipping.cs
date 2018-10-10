using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Framework.Components;
using Grand.Services.Orders;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class EstimateShippingViewComponent : BaseViewComponent
    {
        private readonly IShoppingCartViewModelService _shoppingCartViewModelService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        public EstimateShippingViewComponent(IShoppingCartViewModelService shoppingCartViewModelService, IWorkContext workContext, IStoreContext storeContext)
        {
            this._shoppingCartViewModelService = shoppingCartViewModelService;
            this._workContext = workContext;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke(bool? prepareAndDisplayOrderReviewData)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var model = _shoppingCartViewModelService.PrepareEstimateShipping(cart);
            if (!model.Enabled)
                return Content("");

            return View(model);
        }

    }
}