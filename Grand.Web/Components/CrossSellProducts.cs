using System.Linq;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Core;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Core.Domain.Orders;
using Grand.Services.Orders;

namespace Grand.Web.Components
{
    public class CrossSellProductsViewComponent : ViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductWebService _productWebService;
        private readonly IStoreContext _storeContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        #endregion

        #region Constructors

        public CrossSellProductsViewComponent(
            IProductService productService,
            IWorkContext workContext,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductWebService productWebService,
            IStoreContext storeContext,
            ShoppingCartSettings shoppingCartSettings
)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._aclService = aclService;
            this._shoppingCartSettings = shoppingCartSettings;
            this._productWebService = productWebService;
            this._storeMappingService = storeMappingService;
            this._storeContext = storeContext;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var products = _productService.GetCrosssellProductsByShoppingCart(cart, _shoppingCartSettings.CrossSellsNumber);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            var model = _productWebService.PrepareProductOverviewModels(products,
                productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true)
                .ToList();

            return View(model);

        }

        #endregion

    }
}
