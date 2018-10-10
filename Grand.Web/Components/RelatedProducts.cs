using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Components
{
    public class RelatedProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductViewModelService _productViewModelService;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        #endregion

        #region Constructors

        public RelatedProductsViewComponent(
            IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductViewModelService productViewModelService,
            ICacheManager cacheManager,
            IStoreContext storeContext)
        {
            this._productService = productService;
            this._aclService = aclService;
            this._productViewModelService = productViewModelService;
            this._storeMappingService = storeMappingService;
            this._cacheManager = cacheManager;
            this._storeContext = storeContext;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(string productId, int? productThumbPictureSize)
        {
            //load and cache report
            var productIds = _cacheManager.Get(string.Format(ModelCacheEventConsumer.PRODUCTS_RELATED_IDS_KEY, productId, _storeContext.CurrentStore.Id),
                () =>
                    _productService.GetProductById(productId).RelatedProducts.Select(x => x.ProductId2).ToArray()
                    );

            //load products
            var products = _productService.GetProductsByIds(productIds);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            var model = _productViewModelService.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();
            return View(model);
        }

        #endregion

    }
}
