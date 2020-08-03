using Grand.Core;
using Grand.Domain.Orders;
using Grand.Framework.Components;
using Grand.Services.Security;
using Grand.Web.Features.Models.ShoppingCart;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class FlyoutShoppingCartViewComponent : BaseViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public FlyoutShoppingCartViewComponent(IMediator mediator,
            IPermissionService permissionService,
            ShoppingCartSettings shoppingCartSettings,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _mediator = mediator;
            _permissionService = permissionService;
            _shoppingCartSettings = shoppingCartSettings;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");

            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return Content("");

            var model = await _mediator.Send(new GetMiniShoppingCart() {
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                TaxDisplayType = _workContext.TaxDisplayType,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }
    }
}