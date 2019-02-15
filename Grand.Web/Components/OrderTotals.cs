using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Framework.Components;
using Grand.Services.Orders;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class OrderTotalsViewComponent : BaseViewComponent
    {
        private readonly IShoppingCartViewModelService _shoppingCartViewModelService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        public OrderTotalsViewComponent(IShoppingCartViewModelService shoppingCartViewModelService, IWorkContext workContext, IStoreContext storeContext)
        {
            this._shoppingCartViewModelService = shoppingCartViewModelService;
            this._workContext = workContext;
            this._storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool isEditable)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = await Task.Run(() => _shoppingCartViewModelService.PrepareOrderTotals(cart, isEditable));
            return View(model);
        }
    }
}