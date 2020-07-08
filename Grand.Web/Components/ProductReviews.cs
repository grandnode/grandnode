using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Web.Features.Models.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class ProductReviewsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IMediator _mediator;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Constructors

        public ProductReviewsViewComponent(
            IProductService productService,
            IMediator mediator,
            IWorkContext workContext,
            IStoreContext storeContext,
            CatalogSettings catalogSettings)
        {
            _productService = productService;
            _workContext = workContext;
            _storeContext = storeContext;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker
        public async Task<IViewComponentResult> InvokeAsync(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null || !product.Published || !product.AllowCustomerReviews)
                return Content("");

            var model = await _mediator.Send(new GetProductReviews() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Product = product,
                Store = _storeContext.CurrentStore,
                Size = _catalogSettings.NumberOfReview
            });

            return View(model);
        }

        #endregion

    }
}
