using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.DiscountRules.HasOneProduct
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.DiscountRules.HasOneProduct.Configure",
                 "Admin/DiscountRulesHasOneProduct/Configure",
                 new { controller = "DiscountRulesHasOneProduct", action = "Configure" }                 
            );
            routeBuilder.MapRoute("Plugin.DiscountRules.HasOneProduct.ProductAddPopup",
                 "Admin/DiscountRulesHasOneProduct/ProductAddPopup",
                 new { controller = "DiscountRulesHasOneProduct", action = "ProductAddPopup" }                 
            );
            routeBuilder.MapRoute("Plugin.DiscountRules.HasOneProduct.ProductAddPopupList",
                 "Admin/DiscountRulesHasOneProduct/ProductAddPopupList",
                 new { controller = "DiscountRulesHasOneProduct", action = "ProductAddPopupList" }                 
            );
            routeBuilder.MapRoute("Plugin.DiscountRules.HasOneProduct.LoadProductFriendlyNames",
                 "Admin/DiscountRulesHasOneProduct/LoadProductFriendlyNames",
                 new { controller = "DiscountRulesHasOneProduct", action = "LoadProductFriendlyNames" }                 
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
