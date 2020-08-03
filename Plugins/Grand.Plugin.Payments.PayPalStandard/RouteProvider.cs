using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.Payments.PayPalStandard
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            //PDT
            routeBuilder.MapControllerRoute("Plugin.Payments.PayPalStandard.PDTHandler",
                 "Plugins/PaymentPayPalStandard/PDTHandler",
                 new { controller = "PaymentPayPalStandard", action = "PDTHandler" }
            );
            //IPN
            routeBuilder.MapControllerRoute("Plugin.Payments.PayPalStandard.IPNHandler",
                 "Plugins/PaymentPayPalStandard/IPNHandler",
                 new { controller = "PaymentPayPalStandard", action = "IPNHandler" }
            );
            //Cancel
            routeBuilder.MapControllerRoute("Plugin.Payments.PayPalStandard.CancelOrder",
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
