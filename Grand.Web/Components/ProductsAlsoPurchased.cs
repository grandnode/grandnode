using System.Linq;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Core;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Core.Caching;
using Grand.Services.Orders;
using Grand.Web.Infrastructure.Cache;

namespace Grand.Web.Components
{
    public class ProductsAlsoPurchasedViewComponent : ViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductWebService _productWebService;
        private readonly ICacheManager _cacheManager;
        private readonly IOrderReportService _orderReportService;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public ProductsAlsoPurchasedViewComponent(
            IProductService productService,
            IWorkContext workContext,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductWebService productWebService,
            ICacheManager cacheManager,
            IOrderReportService orderReportService,
            IStoreContext storeContext,
            CatalogSettings catalogSettings
)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
            this._productWebService = productWebService;
            this._storeMappingService = storeMappingService;
            this._cacheManager = cacheManager;
            this._orderReportService = orderReportService;
            this._storeContext = storeContext;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(string productId, int? productThumbPictureSize)
        {
            if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
                return Content("");

            //load and cache report
            var productIds = _cacheManager.Get(string.Format(ModelCacheEventConsumer.PRODUCTS_ALSO_PURCHASED_IDS_KEY, productId, _storeContext.CurrentStore.Id),
                () =>
                    _orderReportService
                    .GetAlsoPurchasedProductsIds(_storeContext.CurrentStore.Id, productId, _catalogSettings.ProductsAlsoPurchasedNumber)
                    );

            //load products
            var products = _productService.GetProductsByIds(productIds);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            //prepare model
            var model = _productWebService.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();

            return View(model);
        }

        #endregion

    }
}
