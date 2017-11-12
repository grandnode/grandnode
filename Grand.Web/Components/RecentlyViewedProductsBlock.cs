using System.Linq;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Core;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Components
{
    public class RecentlyViewedProductsBlockViewComponent : ViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductWebService _productWebService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public RecentlyViewedProductsBlockViewComponent(
            IProductService productService,
            IWorkContext workContext,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductWebService productWebService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            CatalogSettings catalogSettings
)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
            this._recentlyViewedProductsService = recentlyViewedProductsService;
            this._storeMappingService = storeMappingService;
            this._productWebService = productWebService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(int? productThumbPictureSize, bool? preparePriceModel)
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var preparePictureModel = productThumbPictureSize.HasValue;
            var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_workContext.CurrentCustomer.Id, _catalogSettings.RecentlyViewedProductsNumber);

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            //prepare model
            var model = new List<ProductOverviewModel>();
            model.AddRange(_productWebService.PrepareProductOverviewModels(products,
                preparePriceModel.GetValueOrDefault(),
                preparePictureModel,
                productThumbPictureSize));

            return View(model);

        }

        #endregion

    }
}
