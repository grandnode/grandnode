using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.DiscountRules.HasAllProducts
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.DiscountRules.HasAllProducts.Configure",
                 "Admin/DiscountRulesHasAllProducts/Configure",
                 new { controller = "DiscountRulesHasAllProducts", action = "Configure" }
            );
            routeBuilder.MapRoute("Plugin.DiscountRules.HasAllProducts.ProductAddPopup",
                 "Admin/DiscountRulesHasAllProducts/ProductAddPopup",
                 new { controller = "DiscountRulesHasAllProducts", action = "ProductAddPopup" }
            );
            routeBuilder.MapRoute("Plugin.DiscountRules.HasAllProducts.ProductAddPopupList",
                 "Admin/DiscountRulesHasAllProducts/ProductAddPopupList",
                 new { controller = "DiscountRulesHasAllProducts", action = "ProductAddPopupList" }
            );
            routeBuilder.MapRoute("Plugin.DiscountRules.HasAllProducts.LoadProductFriendlyNames",
                 "Admin/DiscountRulesHasAllProducts/LoadProductFriendlyNames",
                 new { controller = "DiscountRulesHasAllProducts", action = "LoadProductFriendlyNames" }
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
