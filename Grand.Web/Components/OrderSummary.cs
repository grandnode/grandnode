using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Framework.Components;
using Grand.Services.Orders;
using Grand.Web.Models.ShoppingCart;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class OrderSummaryViewComponent : BaseViewComponent
    {
        private readonly IShoppingCartViewModelService _shoppingCartViewModelService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        public OrderSummaryViewComponent(IShoppingCartViewModelService shoppingCartViewModelService, IWorkContext workContext, IStoreContext storeContext)
        {
            this._shoppingCartViewModelService = shoppingCartViewModelService;
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
            _shoppingCartViewModelService.PrepareShoppingCart(model, cart,
                isEditable: false,
                prepareAndDisplayOrderReviewData: prepareAndDisplayOrderReviewData.GetValueOrDefault());
            return View(model);

        }
    }
}