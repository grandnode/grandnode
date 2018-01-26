using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.Payments.PayPalStandard
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //PDT
            routeBuilder.MapRoute("Plugin.Payments.PayPalStandard.PDTHandler",
                 "Plugins/PaymentPayPalStandard/PDTHandler",
                 new { controller = "PaymentPayPalStandard", action = "PDTHandler" }
            );
            //IPN
            routeBuilder.MapRoute("Plugin.Payments.PayPalStandard.IPNHandler",
                 "Plugins/PaymentPayPalStandard/IPNHandler",
                 new { controller = "PaymentPayPalStandard", action = "IPNHandler" }
            );
            //Cancel
            routeBuilder.MapRoute("Plugin.Payments.PayPalStandard.CancelOrder",
                 "Plugins/PaymentPayPalStandard/CancelOrder",
                 new { controller = "PaymentPayPalStandard", action = "CancelOrder" }
            );
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
