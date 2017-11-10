using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Core.Infrastructure;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.Widgets.GoogleAnalytics.Models;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Grand.Plugin.Widgets.GoogleAnalytics.Controllers
{
    [Area("Admin")]
    public class WidgetsGoogleAnalyticsController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;
        private readonly ICategoryService _categoryService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILocalizationService _localizationService;

        public WidgetsGoogleAnalyticsController(IWorkContext workContext,
            IStoreContext storeContext, 
            IStoreService storeService,
            ISettingService settingService, 
            IOrderService orderService, 
            ILogger logger, 
            ICategoryService categoryService,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._orderService = orderService;
            this._productService = productService;
            this._logger = logger;
            this._categoryService = categoryService;
            this._productAttributeParser = productAttributeParser;
            this._localizationService = localizationService;
        }

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsEcommerceSettings>(storeScope);
            var model = new ConfigurationModel();
            model.GoogleId = googleAnalyticsSettings.GoogleId;
            model.TrackingScript = googleAnalyticsSettings.TrackingScript;
            model.EcommerceScript = googleAnalyticsSettings.EcommerceScript;
            model.EcommerceDetailScript = googleAnalyticsSettings.EcommerceDetailScript;
            model.IncludingTax = googleAnalyticsSettings.IncludingTax;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.GoogleId_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.GoogleId, storeScope);
                model.TrackingScript_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.TrackingScript, storeScope);
                model.EcommerceScript_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.EcommerceScript, storeScope);
                model.EcommerceDetailScript_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.EcommerceDetailScript, storeScope);
                model.IncludingTax_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.IncludingTax, storeScope);
            }

            return View("~/Plugins/Widgets.GoogleAnalytics/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsEcommerceSettings>(storeScope);
            googleAnalyticsSettings.GoogleId = model.GoogleId;
            googleAnalyticsSettings.TrackingScript = model.TrackingScript;
            googleAnalyticsSettings.EcommerceScript = model.EcommerceScript;
            googleAnalyticsSettings.EcommerceDetailScript = model.EcommerceDetailScript;
            googleAnalyticsSettings.IncludingTax = model.IncludingTax;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.GoogleId_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(googleAnalyticsSettings, x => x.GoogleId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(googleAnalyticsSettings, x => x.GoogleId, storeScope);
            
            if (model.TrackingScript_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(googleAnalyticsSettings, x => x.TrackingScript, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(googleAnalyticsSettings, x => x.TrackingScript, storeScope);
            
            if (model.EcommerceScript_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(googleAnalyticsSettings, x => x.EcommerceScript, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(googleAnalyticsSettings, x => x.EcommerceScript, storeScope);
            
            if (model.EcommerceDetailScript_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(googleAnalyticsSettings, x => x.EcommerceDetailScript, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(googleAnalyticsSettings, x => x.EcommerceDetailScript, storeScope);

            if (model.IncludingTax_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(googleAnalyticsSettings, x => x.IncludingTax, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(googleAnalyticsSettings, x => x.IncludingTax, storeScope);
            
            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
        
        public IActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            string globalScript = "";
            var routeData = Url.ActionContext.RouteData;

            try
            {
                var controller = routeData.Values["controller"];
                var action = routeData.Values["action"];

                if (controller == null || action == null)
                    return Content("");

                //Special case, if we are in last step of checkout, we can use order total for conversion value
                if (controller.ToString().Equals("checkout", StringComparison.OrdinalIgnoreCase) &&
                    action.ToString().Equals("completed", StringComparison.OrdinalIgnoreCase))
                {
                    var lastOrder = GetLastOrder();
                    globalScript += GetEcommerceScript(lastOrder);
                }
                else
                {
                    globalScript += GetTrackingScript();
                }
            }
            catch (Exception ex)
            {
                _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "Error creating scripts for google ecommerce tracking", ex.ToString());
            }
            return Content(globalScript);
        }

        private Order GetLastOrder()
        {
            var order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1).FirstOrDefault();
            return order;
        }
        
        private string GetTrackingScript()
        {
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsEcommerceSettings>(_storeContext.CurrentStore.Id);
            var analyticsTrackingScript = googleAnalyticsSettings.TrackingScript + "\n";
            analyticsTrackingScript = analyticsTrackingScript.Replace("{GOOGLEID}", googleAnalyticsSettings.GoogleId);
            analyticsTrackingScript = analyticsTrackingScript.Replace("{ECOMMERCE}", "");
            return analyticsTrackingScript;
        }
        
        private string GetEcommerceScript(Order order)
        {
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsEcommerceSettings>(_storeContext.CurrentStore.Id);
            var usCulture = new CultureInfo("en-US");
            var analyticsTrackingScript = googleAnalyticsSettings.TrackingScript + "\n";
            analyticsTrackingScript = analyticsTrackingScript.Replace("{GOOGLEID}", googleAnalyticsSettings.GoogleId);

            if (order != null)
            {
                var analyticsEcommerceScript = googleAnalyticsSettings.EcommerceScript + "\n";
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{GOOGLEID}", googleAnalyticsSettings.GoogleId);
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{ORDERID}", order.Id.ToString());
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{SITE}", _storeContext.CurrentStore.Url.Replace("http://", "").Replace("/", ""));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{TOTAL}", order.OrderTotal.ToString("0.00", usCulture));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{TAX}", order.OrderTax.ToString("0.00", usCulture));
                var orderShipping = googleAnalyticsSettings.IncludingTax ? order.OrderShippingInclTax : order.OrderShippingExclTax;
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{SHIP}", orderShipping.ToString("0.00", usCulture));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{CITY}", order.BillingAddress == null ? "" : FixIllegalJavaScriptChars(order.BillingAddress.City));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{STATEPROVINCE}", order.BillingAddress == null || String.IsNullOrEmpty(order.BillingAddress.StateProvinceId) ? "" : FixIllegalJavaScriptChars(EngineContext.Current.Resolve<IStateProvinceService>().GetStateProvinceById(order.BillingAddress.StateProvinceId).Name));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{COUNTRY}", order.BillingAddress == null || String.IsNullOrEmpty(order.BillingAddress.CountryId) ? "" : FixIllegalJavaScriptChars(EngineContext.Current.Resolve<ICountryService>().GetCountryById(order.BillingAddress.CountryId).Name));

                var sb = new StringBuilder();
                foreach (var item in order.OrderItems)
                {
                    var product = _productService.GetProductById(item.ProductId);
                    string analyticsEcommerceDetailScript = googleAnalyticsSettings.EcommerceDetailScript;
                    //get category
                    string category = "";
                    if (product.ProductCategories.FirstOrDefault() != null)
                    {
                        var defaultProductCategory = _categoryService.GetCategoryById(product.ProductCategories.FirstOrDefault().CategoryId); //_categoryService.GetProductCategoriesByProductId(item.ProductId).FirstOrDefault();
                        if (defaultProductCategory != null)
                            category = defaultProductCategory.Name;
                    }
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{ORDERID}", order.Id.ToString());
                    //The SKU code is a required parameter for every item that is added to the transaction
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{PRODUCTSKU}", FixIllegalJavaScriptChars(product.FormatSku(item.AttributesXml, _productAttributeParser)));
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{PRODUCTNAME}", FixIllegalJavaScriptChars(product.Name));
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{CATEGORYNAME}", FixIllegalJavaScriptChars(category));
                    var unitPrice = googleAnalyticsSettings.IncludingTax ? item.UnitPriceInclTax : item.UnitPriceExclTax;
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{UNITPRICE}", unitPrice.ToString("0.00", usCulture));
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{QUANTITY}", item.Quantity.ToString());
                    sb.AppendLine(analyticsEcommerceDetailScript);
                }

                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{DETAILS}", sb.ToString());

                analyticsTrackingScript = analyticsTrackingScript.Replace("{ECOMMERCE}", analyticsEcommerceScript);
            }
            return analyticsTrackingScript;
        }

        private string FixIllegalJavaScriptChars(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            //replace ' with \' (http://stackoverflow.com/questions/4292761/need-to-url-encode-labels-when-tracking-events-with-google-analytics)
            text = text.Replace("'", "\\'");
            return text;
        }
    }
}