using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Orders;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Features.Models.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class CrossSellProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IAclService _aclService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IMediator _mediator;

        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        #endregion

        #region Constructors

        public CrossSellProductsViewComponent(
            IProductService productService,
            IAclService aclService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IStoreMappingService storeMappingService,
            IMediator mediator,
            CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings)
        {
            _productService = productService;
            _aclService = aclService;
            _storeContext = storeContext;
            _workContext = workContext;
            _storeMappingService = storeMappingService;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            if (_shoppingCartSettings.CrossSellsNumber == 0)
                return Content("");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, _storeContext.CurrentStore.Id)
                .ToList();

            var products = await _productService.GetCrosssellProductsByShoppingCart(cart, _shoppingCartSettings.CrossSellsNumber);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            var model = await _mediator.Send(new GetProductOverview() {
                PrepareSpecificationAttributes = _catalogSettings.ShowSpecAttributeOnCatalogPages,
                ProductThumbPictureSize = productThumbPictureSize,
                Products = products,
                ForceRedirectionAfterAddingToCart = true,
            });

            return View(model);

        }

        #endregion

    }
}
