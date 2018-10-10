using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Components
{
    public class HomePageBestSellersViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductViewModelService _productViewModelService;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly IOrderReportService _orderReportService;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public HomePageBestSellersViewComponent(
            IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductViewModelService productViewModelService,
            IStoreContext storeContext,
            ICacheManager cacheManager,
            IOrderReportService orderReportService,
            CatalogSettings catalogSettings
)
        {
            this._productService = productService;
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
            this._productViewModelService = productViewModelService;
            this._storeMappingService = storeMappingService;
            this._storeContext = storeContext;
            this._orderReportService = orderReportService;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
                return Content("");

            //load and cache report
            var report = _cacheManager.Get(string.Format(ModelCacheEventConsumer.HOMEPAGE_BESTSELLERS_IDS_KEY, _storeContext.CurrentStore.Id),
                () => _orderReportService.BestSellersReport(
                        storeId: _storeContext.CurrentStore.Id,
                        pageSize: _catalogSettings.NumberOfBestsellersOnHomepage)
                        .ToList());


            //load products
            var products = _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            //prepare model
            var model = _productViewModelService.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();
            return View(model);

        }

        #endregion

    }
}
