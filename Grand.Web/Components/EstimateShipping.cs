using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.Orders;
using Grand.Core;
using Grand.Services.Orders;

namespace Grand.Web.ViewComponents
{
    public class EstimateShippingViewComponent : ViewComponent
    {
        private readonly IShoppingCartWebService _shoppingCartWebService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        public EstimateShippingViewComponent(IShoppingCartWebService shoppingCartWebService, IWorkContext workContext, IStoreContext storeContext)
        {
            this._shoppingCartWebService = shoppingCartWebService;
            this._workContext = workContext;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke(bool? prepareAndDisplayOrderReviewData)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var model = _shoppingCartWebService.PrepareEstimateShipping(cart);
            if (!model.Enabled)
                return Content("");

            return View(model);
        }

    }
}