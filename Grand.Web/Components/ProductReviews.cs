using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Web.Models.Catalog;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Components
{
    public class ProductReviewsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IProductViewModelService _productViewModelService;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public ProductReviewsViewComponent(
            IProductService productService,
            IProductViewModelService productViewModelService,
            CatalogSettings catalogSettings)
        {
            this._productService = productService;
            this._catalogSettings = catalogSettings;
            this._productViewModelService = productViewModelService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !product.Published || !product.AllowCustomerReviews)
                return Content("");

            var model = new ProductReviewsModel();
            _productViewModelService.PrepareProductReviewsModel(model, product, _catalogSettings.NumberOfReview);
            
            return View(model);
        }

        #endregion

    }
}
