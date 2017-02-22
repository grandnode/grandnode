using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework.Mvc.Routes;

namespace Grand.Plugin.Payments.PayPalDirect
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.PayPalDirect.Webhook",
                "Plugins/PaymentPayPalDirect/Webhook",
                new { controller = "PaymentPayPalDirect", action = "WebhookEventsHandler" },
                new[] { "Grand.Plugin.Payments.PayPalDirect.Controllers" });
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
