using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework.Mvc.Routes;

namespace Grand.Plugin.Shipping.ShippingPoint
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Shipping.ShippingPoint.Create",
                 "Plugins/ShippingPoint/Create",
                 new { controller = "ShippingPoint", action = "Create", },
                 new[] { "Grand.Plugin.Shipping.ShippingPoint.Controllers" }
            );

            routes.MapRoute("Plugin.Shipping.ShippingPoint.Edit",
                 "Plugins/ShippingPoint/Edit",
                 new { controller = "ShippingPoint", action = "Edit" },
                 new[] { "Grand.Plugin.Shipping.ShippingPoint.Controllers" }
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