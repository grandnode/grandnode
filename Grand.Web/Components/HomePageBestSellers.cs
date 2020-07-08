using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Features.Models.Products;
using Grand.Web.Infrastructure.Cache;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class HomePageBestSellersViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly IOrderReportService _orderReportService;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly IMediator _mediator;

        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public HomePageBestSellersViewComponent(
            IOrderReportService orderReportService,
            ICacheManager cacheManager,
            IStoreContext storeContext,
            IProductService productService,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            _orderReportService = orderReportService;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _productService = productService;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
        }


        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
                return Content("");

            //load and cache report
            var fromdate = DateTime.UtcNow.AddMonths(_catalogSettings.PeriodBestsellers > 0 ? -_catalogSettings.PeriodBestsellers : -12);
            var report = await _cacheManager.GetAsync(string.Format(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_KEY, _storeContext.CurrentStore.Id), async () =>
                                await _orderReportService.BestSellersReport(
                                    createdFromUtc: fromdate,
                                    ps: Domain.Payments.PaymentStatus.Paid,
                                    storeId: _storeContext.CurrentStore.Id,
                                    pageSize: _catalogSettings.NumberOfBestsellersOnHomepage));

            //load products
            var products = await _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());
            
            if (!products.Any())
                return Content("");

            var model = await _mediator.Send(new GetProductOverview() {
                ProductThumbPictureSize = productThumbPictureSize,
                Products = products,
            });

            return View(model);
        }

        #endregion

    }
}
