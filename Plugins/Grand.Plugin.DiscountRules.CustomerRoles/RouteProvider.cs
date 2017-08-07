using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.DiscountRules.CustomerRoles
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.DiscountRules.CustomerRoles.Configure",
                 "Plugins/DiscountRulesCustomerRoles/Configure",
                 new { controller = "DiscountRulesCustomerRoles", action = "Configure" }
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
