using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Components
{
    public class PersonalizedProductsViewComponent : BaseViewComponent
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

        public PersonalizedProductsViewComponent(
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
            this._productViewModelService = productViewModelService;
            this._storeMappingService = storeMappingService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            if (!_catalogSettings.PersonalizedProductsEnabled || _catalogSettings.PersonalizedProductsNumber == 0)
                return Content("");

            var customer = _workContext.CurrentCustomer;
            var products = _productService.GetPersonalizedProducts(customer.Id);

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p, customer) && _storeMappingService.Authorize(p)).ToList();

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
