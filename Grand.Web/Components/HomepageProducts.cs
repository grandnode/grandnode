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
    public class HomePageProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductViewModelService _productViewModelService;
        #endregion

        #region Constructors

        public HomePageProductsViewComponent(
            IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductViewModelService productViewModelService)
        {
            this._productService = productService;
            this._aclService = aclService;
            this._productViewModelService = productViewModelService;
            this._storeMappingService = storeMappingService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            var products = _productService.GetAllProductsDisplayedOnHomePage();

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
