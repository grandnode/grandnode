using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Orders;
using Grand.Web.Features.Models.Products;
using Grand.Web.Infrastructure.Cache;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class ProductsAlsoPurchasedViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;
        private readonly IOrderReportService _orderReportService;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public ProductsAlsoPurchasedViewComponent(
            IProductService productService,
            IMediator mediator,
            ICacheManager cacheManager,
            IOrderReportService orderReportService,
            IStoreContext storeContext,
            CatalogSettings catalogSettings
)
        {
            _productService = productService;
            _catalogSettings = catalogSettings;
            _mediator = mediator;
            _cacheManager = cacheManager;
            _orderReportService = orderReportService;
            _storeContext = storeContext;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(string productId, int? productThumbPictureSize)
        {
            if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
                return Content("");

            //load and cache report
            var productIds = await _cacheManager.GetAsync(string.Format(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_KEY, productId, _storeContext.CurrentStore.Id),
                () =>
                    _orderReportService
                    .GetAlsoPurchasedProductsIds(_storeContext.CurrentStore.Id, productId, _catalogSettings.ProductsAlsoPurchasedNumber)
                    );

            //load products
            var products = await _productService.GetProductsByIds(productIds);

            if (!products.Any())
                return Content("");

            //prepare model
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
