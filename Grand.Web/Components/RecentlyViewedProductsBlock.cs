using Grand.Core;
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
    public class RecentlyViewedProductsBlockViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IMediator _mediator;

        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public RecentlyViewedProductsBlockViewComponent(
            IWorkContext workContext,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            _workContext = workContext;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize, bool? preparePriceModel)
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var preparePictureModel = productThumbPictureSize.HasValue;
            var products = await _recentlyViewedProductsService.GetRecentlyViewedProducts(_workContext.CurrentCustomer.Id, _catalogSettings.RecentlyViewedProductsNumber);

            if (!products.Any())
                return Content("");

            var model = await _mediator.Send(new GetProductOverview() {
                PreparePictureModel = preparePictureModel,
                PreparePriceModel = preparePriceModel.GetValueOrDefault(),
                PrepareSpecificationAttributes = _catalogSettings.ShowSpecAttributeOnCatalogPages,
                ProductThumbPictureSize = productThumbPictureSize,
                Products = products
            });

            return View(model);
        }

        #endregion

    }
}
