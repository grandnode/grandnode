using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Seo;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Web.Areas.Admin.Models.Home;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class HomeController : BaseAdminController
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
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

        public HomeController(
            ILocalizationService localizationService,
            IStoreContext storeContext, 
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
            this._localizationService = localizationService;
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
            string vendorId = "";
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            model.OrdersPending = _orderReportService.GetOrderAverageReportLine(os: Core.Domain.Orders.OrderStatus.Pending).CountOrders;
            model.AbandonedCarts = _customerService.GetAllCustomers(loadOnlyWithShoppingCart: true, pageSize: 1).TotalCount;

            IList<Product> products;
            IList<ProductAttributeCombination> combinations;
            Grand.Core.Infrastructure.EngineContext.Current.Resolve<Grand.Services.Catalog.IProductService>()
                .GetLowStockProducts(vendorId, out products, out combinations);

            model.LowStockProducts = products.Count + combinations.Count;

            model.ReturnRequests = (int)_returnRequestRepository.Table.Where(x=>x.ReturnRequestStatusId == 0).Count();
            model.TodayRegisteredCustomers = _customerService.GetAllCustomers(customerRoleIds: new string[] { _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id }, createdFromUtc: DateTime.UtcNow.Date, pageSize: 1).TotalCount;
            return model;

        }

        #endregion

        #region Methods

        public IActionResult Index()
        {
            var model = new DashboardModel();
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            if (string.IsNullOrEmpty(_googleAnalyticsSettings.gaprivateKey) || 
                string.IsNullOrEmpty(_googleAnalyticsSettings.gaserviceAccountEmail) ||
                string.IsNullOrEmpty(_googleAnalyticsSettings.gaviewID))
                model.HideReportGA = true;

            return View(model);
        }

        public IActionResult Statistics()
        {
            var model = new DashboardModel();
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            return View(model);
        }

        public IActionResult DashboardActivity()
        {
            var model = PrepareActivityModel();
            return PartialView(model);
        }

        public IActionResult SetLanguage(string langid, [FromServices] ILanguageService languageService, string returnUrl = "")
        {
            var language = languageService.GetLanguageById(langid);
            if (language != null)
            {
                _workContext.WorkingLanguage = language;
            }

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.Action("Index", "Home", new { area = "Admin" });
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            return Redirect(returnUrl);
        }
        [AcceptVerbs("Get")]
        public IActionResult GetStatesByCountryId([FromServices] ICountryService countryService, [FromServices] IStateProvinceService stateProvinceService,
            string countryId, bool? addSelectStateItem, bool? addAsterisk)
        {
            // This action method gets called via an ajax request
            if (String.IsNullOrEmpty(countryId))
                return Json(new List<dynamic>() { new { id = "", name = _localizationService.GetResource("Address.SelectState") } });

            var country = countryService.GetCountryById(countryId);
            var states = country != null ? stateProvinceService.GetStateProvincesByCountryId(country.Id, showHidden: true).ToList() : new List<StateProvince>();
            var result = (from s in states
                          select new { id = s.Id, name = s.Name }).ToList();
            if (addAsterisk.HasValue && addAsterisk.Value)
            {
                //asterisk
                result.Insert(0, new { id = "", name = "*" });
            }
            else
            {
                if (country == null)
                {
                    //country is not selected ("choose country" item)
                    if (addSelectStateItem.HasValue && addSelectStateItem.Value)
                    {
                        result.Insert(0, new { id = "", name = _localizationService.GetResource("Admin.Address.SelectState") });
                    }
                    else
                    {
                        result.Insert(0, new { id = "", name = _localizationService.GetResource("Admin.Address.OtherNonUS") });
                    }
                }
                else
                {
                    //some country is selected
                    if (result.Count == 0)
                    {
                        //country does not have states
                        result.Insert(0, new { id = "", name = _localizationService.GetResource("Admin.Address.OtherNonUS") });
                    }
                    else
                    {
                        //country has some states
                        if (addSelectStateItem.HasValue && addSelectStateItem.Value)
                        {
                            result.Insert(0, new { id = "", name = _localizationService.GetResource("Admin.Address.SelectState") });
                        }
                    }
                }
            }
            return Json(result);
        }

        #endregion
    }
}
