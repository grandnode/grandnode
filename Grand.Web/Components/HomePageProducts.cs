using Grand.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Web.Features.Models.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class HomePageProductsViewComponent : BaseViewComponent
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IMediator _mediator;

        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Constructors
        public HomePageProductsViewComponent(
            IProductService productService,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            _productService = productService;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            var products = await _productService.GetAllProductsDisplayedOnHomePage();

            if (!products.Any())
                return Content("");

            var model = await _mediator.Send(new GetProductOverview() {
                PreparePictureModel = true,
                PreparePriceModel = true,
                PrepareSpecificationAttributes = _catalogSettings.ShowSpecAttributeOnCatalogPages,
                ProductThumbPictureSize = productThumbPictureSize,
                Products = products,
            });

            return View(model);
        }

        #endregion

    }
}
