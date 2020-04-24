using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Services.Orders;
using Grand.Web.Models.ShoppingCart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Widgets.FacebookPixel.Components
{
    [ViewComponent(Name = "WidgetsFacebookPixel")]
    public class WidgetsFacebookPixelViewComponent : ViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly FacebookPixelSettings _facebookPixelSettings;

        public WidgetsFacebookPixelViewComponent(IWorkContext workContext,
            IStoreContext storeContext,
            IServiceProvider serviceProvider,
            FacebookPixelSettings facebookPixelSettings
            )
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _serviceProvider = serviceProvider;
            _facebookPixelSettings = facebookPixelSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            //page
            if (widgetZone == FacebookPixelWidgetZone.Page)
            {
                return View("~/Plugins/Widgets.FacebookPixel/Views/PublicInfo.cshtml", GetTrackingScript());
            }
            //add to cart
            if (widgetZone == FacebookPixelWidgetZone.AddToCart)
            {
                var model = additionalData as AddToCartModel;
                if (model != null)
                {
                    return View("~/Plugins/Widgets.FacebookPixel/Views/PublicInfo.cshtml", GetAddToCartScript(model));
                }
            }
            //order details 
            if (widgetZone == FacebookPixelWidgetZone.OrderDetails)
            {
                var orderId = additionalData as string;
                if (!string.IsNullOrEmpty(orderId))
                {
                    return View("~/Plugins/Widgets.FacebookPixel/Views/PublicInfo.cshtml", await GetOrderScript(orderId));
                }

            }
            return Content("");
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
            var trackingScript = _facebookPixelSettings.PixelScript + "\n";
            trackingScript = trackingScript.Replace("{PIXELID}", _facebookPixelSettings.PixelId);
            return trackingScript;
        }

        private string GetAddToCartScript(AddToCartModel model)
        {
            var trackingScript = _facebookPixelSettings.AddToCartScript + "\n";
            trackingScript = trackingScript.Replace("{PRODUCTID}", model.ProductId);
            trackingScript = trackingScript.Replace("{PRODUCTNAME}", model.ProductName);
            trackingScript = trackingScript.Replace("{QTY}", model.Quantity.ToString("N0"));
            trackingScript = trackingScript.Replace("{AMOUNT}", model.DecimalPrice.ToString("F2", CultureInfo.InvariantCulture));
            trackingScript = trackingScript.Replace("{CURRENCY}", _workContext.WorkingCurrency.CurrencyCode);
            return trackingScript;
        }

        private async Task<string> GetOrderScript(string orderId)
        {
            var trackingScript = _facebookPixelSettings.DetailsOrderScript + "\n";
            var orderService = _serviceProvider.GetRequiredService<IOrderService>();
            var order = await orderService.GetOrderById(orderId);
            if (order != null)
            {
                trackingScript = trackingScript.Replace("{AMOUNT}", order.OrderTotal.ToString("F2", CultureInfo.InvariantCulture));
                trackingScript = trackingScript.Replace("{CURRENCY}", order.CustomerCurrencyCode);
            }
            return trackingScript;
        }
    }
}