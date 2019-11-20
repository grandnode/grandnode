using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Web.Models.Catalog;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            _productService = productService;
            _catalogSettings = catalogSettings;
            _productViewModelService = productViewModelService;
        }

        #endregion

        #region Invoker
        public async Task<IViewComponentResult> InvokeAsync(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null || !product.Published || !product.AllowCustomerReviews)
                return Content("");

            var model = new ProductReviewsModel();
            await _productViewModelService.PrepareProductReviewsModel(model, product, _catalogSettings.NumberOfReview);
            
            return View(model);
        }

        #endregion

    }
}
