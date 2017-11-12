using System.Linq;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Core;
using Grand.Services.Security;
using Grand.Services.Stores;

namespace Grand.Web.Components
{
    public class SuggestedProductsViewComponent : ViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductWebService _productWebService;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public SuggestedProductsViewComponent(
            IProductService productService,
            IWorkContext workContext,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductWebService productWebService,
            CatalogSettings catalogSettings
)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
            this._productWebService = productWebService;
            this._storeMappingService = storeMappingService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            if (!_catalogSettings.SuggestedProductsEnabled || _catalogSettings.SuggestedProductsNumber == 0)
                return Content("");

            var products = _productService.GetSuggestedProducts(_workContext.CurrentCustomer.CustomerTags.ToArray());

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            //prepare model
            var model = _productWebService.PrepareProductOverviewModels(products.Take(_catalogSettings.SuggestedProductsNumber), true, true, productThumbPictureSize).ToList();

            return View(model);

        }

        #endregion

    }
}
