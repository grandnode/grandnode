using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;
using Grand.Web.Models.ShoppingCart;
using Grand.Core.Domain.Orders;
using Grand.Core;
using Grand.Services.Orders;

namespace Grand.Web.ViewComponents
{
    public class OrderSummaryViewComponent : ViewComponent
    {
        private readonly IShoppingCartWebService _shoppingCartWebService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        public OrderSummaryViewComponent(IShoppingCartWebService shoppingCartWebService, IWorkContext workContext, IStoreContext storeContext)
        {
            this._shoppingCartWebService = shoppingCartWebService;
            this._workContext = workContext;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke(bool? prepareAndDisplayOrderReviewData, ShoppingCartModel overriddenModel)
        {
            //use already prepared (shared) model
            if (overriddenModel != null)
                return View(overriddenModel);

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();
            _shoppingCartWebService.PrepareShoppingCart(model, cart,
                isEditable: false,
                prepareAndDisplayOrderReviewData: prepareAndDisplayOrderReviewData.GetValueOrDefault());
            return View(model);

        }
    }
}