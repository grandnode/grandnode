using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework.Mvc.Routes;

namespace Grand.Plugin.DiscountRules.HasAllProducts
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HasAllProducts.Configure",
                 "Plugins/DiscountRulesHasAllProducts/Configure",
                 new { controller = "DiscountRulesHasAllProducts", action = "Configure" },
                 new[] { "Grand.Plugin.DiscountRules.HasAllProducts.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasAllProducts.ProductAddPopup",
                 "Plugins/DiscountRulesHasAllProducts/ProductAddPopup",
                 new { controller = "DiscountRulesHasAllProducts", action = "ProductAddPopup" },
                 new[] { "Grand.Plugin.DiscountRules.HasAllProducts.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasAllProducts.ProductAddPopupList",
                 "Plugins/DiscountRulesHasAllProducts/ProductAddPopupList",
                 new { controller = "DiscountRulesHasAllProducts", action = "ProductAddPopupList" },
                 new[] { "Grand.Plugin.DiscountRules.HasAllProducts.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasAllProducts.LoadProductFriendlyNames",
                 "Plugins/DiscountRulesHasAllProducts/LoadProductFriendlyNames",
                 new { controller = "DiscountRulesHasAllProducts", action = "LoadProductFriendlyNames" },
                 new[] { "Grand.Plugin.DiscountRules.HasAllProducts.Controllers" }
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
