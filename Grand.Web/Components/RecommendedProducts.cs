using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Components
{
    public class RecommendedProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductViewModelService _productViewModelService;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public RecommendedProductsViewComponent(
            IProductService productService,
            IWorkContext workContext,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductViewModelService productViewModelService,
            CatalogSettings catalogSettings
)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
            this._storeMappingService = storeMappingService;
            this._productViewModelService = productViewModelService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            if (!_catalogSettings.RecommendedProductsEnabled)
                return Content("");

            var products = _productService.GetRecommendedProducts(_workContext.CurrentCustomer.GetCustomerRoleIds());

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
