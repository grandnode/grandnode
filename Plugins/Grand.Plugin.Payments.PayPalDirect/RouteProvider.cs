using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework.Mvc.Routes;

namespace Grand.Plugin.Payments.PayPalDirect
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //IPN
            routes.MapRoute("Plugin.Payments.PayPalDirect.IPNHandler",
                 "Plugins/PaymentPayPalDirect/IPNHandler",
                 new { controller = "PaymentPayPalDirect", action = "IPNHandler" },
                 new[] { "Grand.Plugin.Payments.PayPalDirect.Controllers" }
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
