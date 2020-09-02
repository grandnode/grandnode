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
    public class EstimateShippingViewComponent : BaseViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public EstimateShippingViewComponent(IMediator mediator, IShoppingCartService shoppingCartService, IStoreContext storeContext, IWorkContext workContext)
        {
            _mediator = mediator;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart);

            var model = await _mediator.Send(new GetEstimateShipping() {
                Cart = cart,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });
            if (!model.Enabled)
                return Content("");

            return View(model);
        }

    }
}
