using Grand.Core;
using Grand.Domain.Orders;
using Grand.Framework.Components;
using Grand.Services.Orders;
using Grand.Web.Features.Models.ShoppingCart;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class OrderTotalsViewComponent : BaseViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public OrderTotalsViewComponent(
            IMediator mediator,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _mediator = mediator;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool isEditable)
        {
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            var model = await _mediator.Send(new GetOrderTotals() {
                Cart = cart,
                IsEditable = isEditable,
                Store = _storeContext.CurrentStore,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                TaxDisplayType = _workContext.TaxDisplayType
            });
            return View(model);
        }
    }
}
