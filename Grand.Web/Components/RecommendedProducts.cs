using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Web.Features.Models.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class RecommendedProductsViewComponent : BaseViewComponent
    {

        #region Fields

        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IMediator _mediator;

        private readonly CatalogSettings _catalogSettings;


        #endregion

        #region Constructors

        public RecommendedProductsViewComponent(
            IProductService productService,
            IWorkContext workContext,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            _productService = productService;
            _workContext = workContext;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            if (!_catalogSettings.RecommendedProductsEnabled)
                return Content("");

            var products = await _productService.GetRecommendedProducts(_workContext.CurrentCustomer.GetCustomerRoleIds());

            if (!products.Any())
                return Content("");

            var model = await _mediator.Send(new GetProductOverview() {
                PreparePictureModel = true,
                PreparePriceModel = true,
                ProductThumbPictureSize = productThumbPictureSize,
                Products = products,
            });

            return View(model);
        }

        #endregion

    }
}
