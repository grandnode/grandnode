using System;
using System.Net;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;
using Grand.Admin.Infrastructure.Cache;
using Grand.Admin.Models.Home;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Common;
using Grand.Services.Configuration;
using Grand.Services.Orders;
using Grand.Services.Customers;
using MongoDB.Bson;
using MongoDB.Driver;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using System.Threading.Tasks;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Seo;

namespace Grand.Admin.Controllers
{
    public partial class HomeController : BaseAdminController
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly CommonSettings _commonSettings;
        private readonly GoogleAnalyticsSettings _googleAnalyticsSettings;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IOrderReportService _orderReportService;
        private readonly ICustomerService _customerService;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ReturnRequest> _returnRequestRepository;

        #endregion

        #region Ctor

        public HomeController(IStoreContext storeContext, 
            CommonSettings commonSettings,
            GoogleAnalyticsSettings googleAnalyticsSettings,
            ISettingService settingService,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IOrderReportService orderReportService,
            ICustomerService customerService,
            IRepository<Product> productRepository,
            IRepository<ReturnRequest> returnRequestRepository)
        {
            this._storeContext = storeContext;
            this._commonSettings = commonSettings;
            this._googleAnalyticsSettings = googleAnalyticsSettings;
            this._settingService = settingService;
            this._workContext = workContext;
            this._cacheManager= cacheManager;
            this._orderReportService = orderReportService;
            this._customerService = customerService;
            this._productRepository = productRepository;
            this._returnRequestRepository = returnRequestRepository;
        }

        #endregion

        #region Utiliti

        private DashboardActivityModel PrepareActivityModel()
        {
            var model = new DashboardActivityModel();
            model.OrdersPending = _orderReportService.GetOrderAverageReportLine(os: Core.Domain.Orders.OrderStatus.Pending).CountOrders;
            model.AbandonedCarts = _customerService.GetAllCustomers(loadOnlyWithShoppingCart: true, pageSize: 1).TotalCount;
            var doc = "{$where: \" this.MinStockQuantity > this.StockQuantity && this.ProductTypeId == 5 && this.ManageInventoryMethodId != 0  \" }";
            model.LowStockProducts = _productRepository.Collection.Find(doc).ToListAsync().Result.Count;

            model.ReturnRequests = (int)_returnRequestRepository.Collection.Count(new BsonDocument());
            model.TodayRegisteredCustomers = _customerService.GetAllCustomers(customerRoleIds: new string[] { _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id }, createdFromUtc: DateTime.UtcNow.Date, pageSize: 1).TotalCount;
            return model;

        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            var model = new DashboardModel();
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            if (string.IsNullOrEmpty(_googleAnalyticsSettings.gaprivateKey) || 
                string.IsNullOrEmpty(_googleAnalyticsSettings.gaserviceAccountEmail) ||
                string.IsNullOrEmpty(_googleAnalyticsSettings.gaviewID))
                model.HideReportGA = true;

            return View(model);
        }

        public ActionResult Statistics()
        {
            var model = new DashboardModel();
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            return View(model);
        }

        public ActionResult DashboardActivity()
        {
            var model = PrepareActivityModel();
            return PartialView(model);
        }

        #endregion
    }
}
