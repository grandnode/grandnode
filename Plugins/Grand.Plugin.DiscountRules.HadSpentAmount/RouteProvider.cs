using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework.Mvc.Routes;

namespace Grand.Plugin.DiscountRules.HadSpentAmount
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HadSpentAmount.Configure",
                 "Plugins/DiscountRulesHadSpentAmount/Configure",
                 new { controller = "DiscountRulesHadSpentAmount", action = "Configure" },
                 new[] { "Grand.Plugin.DiscountRules.HadSpentAmount.Controllers" }
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
