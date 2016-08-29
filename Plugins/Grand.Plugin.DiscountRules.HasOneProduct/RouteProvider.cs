using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework.Mvc.Routes;

namespace Grand.Plugin.DiscountRules.HasOneProduct
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HasOneProduct.Configure",
                 "Plugins/DiscountRulesHasOneProduct/Configure",
                 new { controller = "DiscountRulesHasOneProduct", action = "Configure" },
                 new[] { "Grand.Plugin.DiscountRules.HasOneProduct.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasOneProduct.ProductAddPopup",
                 "Plugins/DiscountRulesHasOneProduct/ProductAddPopup",
                 new { controller = "DiscountRulesHasOneProduct", action = "ProductAddPopup" },
                 new[] { "Grand.Plugin.DiscountRules.HasOneProduct.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasOneProduct.ProductAddPopupList",
                 "Plugins/DiscountRulesHasOneProduct/ProductAddPopupList",
                 new { controller = "DiscountRulesHasOneProduct", action = "ProductAddPopupList" },
                 new[] { "Grand.Plugin.DiscountRules.HasOneProduct.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasOneProduct.LoadProductFriendlyNames",
                 "Plugins/DiscountRulesHasOneProduct/LoadProductFriendlyNames",
                 new { controller = "DiscountRulesHasOneProduct", action = "LoadProductFriendlyNames" },
                 new[] { "Grand.Plugin.DiscountRules.HasOneProduct.Controllers" }
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
