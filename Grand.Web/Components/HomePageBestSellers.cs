using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
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
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IMediator _mediator;

        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Constructors

        public HomePageBestSellersViewComponent(
            IOrderReportService orderReportService,
            ICacheManager cacheManager,
            IStoreContext storeContext,
            IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            _orderReportService = orderReportService;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _productService = productService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
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
                                    ps: Core.Domain.Payments.PaymentStatus.Paid,
                                    storeId: _storeContext.CurrentStore.Id,
                                    pageSize: _catalogSettings.NumberOfBestsellersOnHomepage));

            //load products
            var products = await _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

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
