using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Plugin.Widgets.GoogleAnalytics.Components
{
    [ViewComponent(Name = "WidgetsGoogleAnalytics")]
    public class WidgetsGoogleAnalyticsViewComponent : ViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public WidgetsGoogleAnalyticsViewComponent(IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            ILogger logger,
            IServiceProvider serviceProvider
            )
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            var globalScript = "";
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
                    var lastOrder = await GetLastOrder();
                    globalScript += await GetEcommerceScript(lastOrder);
                }
                else
                {
                    globalScript += GetTrackingScript();
                }
            }
            catch (Exception ex)
            {
                await _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "Error creating scripts for google ecommerce tracking", ex.ToString());
            }

            return View("~/Plugins/Widgets.GoogleAnalytics/Views/PublicInfo.cshtml", globalScript);
        }

        private async Task<Order> GetLastOrder()
        {
            var orderService = _serviceProvider.GetRequiredService<IOrderService>();
            var order = (await orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)).FirstOrDefault();
            return order;
        }

        private string GetTrackingScript()
        {
            var GoogleAnalyticsEcommerceSettings = _settingService.LoadSetting<GoogleAnalyticsEcommerceSettings>(_storeContext.CurrentStore.Id);
            var analyticsTrackingScript = GoogleAnalyticsEcommerceSettings.TrackingScript + "\n";
            analyticsTrackingScript = analyticsTrackingScript.Replace("{GOOGLEID}", GoogleAnalyticsEcommerceSettings.GoogleId);
            analyticsTrackingScript = analyticsTrackingScript.Replace("{ECOMMERCE}", "");
            return analyticsTrackingScript;
        }

        private async Task<string> GetEcommerceScript(Order order)
        {
            var GoogleAnalyticsEcommerceSettings = _settingService.LoadSetting<GoogleAnalyticsEcommerceSettings>(_storeContext.CurrentStore.Id);
            var usCulture = new CultureInfo("en-US");
            var analyticsTrackingScript = GoogleAnalyticsEcommerceSettings.TrackingScript + "\n";
            analyticsTrackingScript = analyticsTrackingScript.Replace("{GOOGLEID}", GoogleAnalyticsEcommerceSettings.GoogleId);

            if (order != null)
            {
                var analyticsEcommerceScript = GoogleAnalyticsEcommerceSettings.EcommerceScript + "\n";
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{GOOGLEID}", GoogleAnalyticsEcommerceSettings.GoogleId);
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{ORDERID}", order.Id.ToString());
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{SITE}", _storeContext.CurrentStore.Url.Replace("http://", "").Replace("/", ""));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{TOTAL}", order.OrderTotal.ToString("0.00", usCulture));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{TAX}", order.OrderTax.ToString("0.00", usCulture));
                var orderShipping = GoogleAnalyticsEcommerceSettings.IncludingTax ? order.OrderShippingInclTax : order.OrderShippingExclTax;
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{SHIP}", orderShipping.ToString("0.00", usCulture));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{CITY}", order.BillingAddress == null ? "" : FixIllegalJavaScriptChars(order.BillingAddress.City));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{STATEPROVINCE}", order.BillingAddress == null || String.IsNullOrEmpty(order.BillingAddress.StateProvinceId) ? "" : FixIllegalJavaScriptChars((await _serviceProvider.GetRequiredService<IStateProvinceService>().GetStateProvinceById(order.BillingAddress.StateProvinceId)).Name));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{COUNTRY}", order.BillingAddress == null || String.IsNullOrEmpty(order.BillingAddress.CountryId) ? "" : FixIllegalJavaScriptChars((await _serviceProvider.GetRequiredService<ICountryService>().GetCountryById(order.BillingAddress.CountryId)).Name));

                var sb = new StringBuilder();

                var productService = _serviceProvider.GetRequiredService<IProductService>();
                var categoryService = _serviceProvider.GetRequiredService<ICategoryService>();
                var productAttributeParser = _serviceProvider.GetRequiredService<IProductAttributeParser>();

                foreach (var item in order.OrderItems)
                {
                    var product = await productService.GetProductById(item.ProductId);
                    string analyticsEcommerceDetailScript = GoogleAnalyticsEcommerceSettings.EcommerceDetailScript;
                    //get category
                    string category = "";
                    if (product.ProductCategories.Any())
                    {
                        var defaultProductCategory = await categoryService.GetCategoryById(product.ProductCategories.OrderBy(x => x.DisplayOrder).FirstOrDefault().CategoryId);
                        if (defaultProductCategory != null)
                            category = defaultProductCategory.Name;
                    }
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{ORDERID}", order.Id.ToString());
                    //The SKU code is a required parameter for every item that is added to the transaction
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{PRODUCTSKU}", FixIllegalJavaScriptChars(product.FormatSku(item.AttributesXml, productAttributeParser)));
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{PRODUCTNAME}", FixIllegalJavaScriptChars(product.Name));
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{CATEGORYNAME}", FixIllegalJavaScriptChars(category));
                    var unitPrice = GoogleAnalyticsEcommerceSettings.IncludingTax ? item.UnitPriceInclTax : item.UnitPriceExclTax;
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