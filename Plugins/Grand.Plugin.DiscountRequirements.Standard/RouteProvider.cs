using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.DiscountRequirements
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //CustomerRoles
            routeBuilder.MapRoute("Plugin.DiscountRequirements.CustomerRoles.Configure",
                 "Admin/CustomerRoles/Configure",
                 new { controller = "CustomerRoles", action = "Configure" }
            );

            //HadSpentAmount
            routeBuilder.MapRoute("Plugin.DiscountRequirements.HadSpentAmount.Configure",
                 "Admin/HadSpentAmount/Configure",
                 new { controller = "HadSpentAmount", action = "Configure" }
            );

            //HasAllProducts
            routeBuilder.MapRoute("Plugin.DiscountRequirements.HasAllProducts.Configure",
                 "Admin/HasAllProducts/Configure",
                 new { controller = "HasAllProducts", action = "Configure" }
            );
            routeBuilder.MapRoute("Plugin.DiscountRequirements.HasAllProducts.ProductAddPopup",
                 "Admin/HasAllProducts/ProductAddPopup",
                 new { controller = "HasAllProducts", action = "ProductAddPopup" }
            );
            routeBuilder.MapRoute("Plugin.DiscountRequirements.HasAllProducts.ProductAddPopupList",
                 "Admin/HasAllProducts/ProductAddPopupList",
                 new { controller = "HasAllProducts", action = "ProductAddPopupList" }
            );
            routeBuilder.MapRoute("Plugin.DiscountRequirements.HasAllProducts.LoadProductFriendlyNames",
                 "Admin/HasAllProducts/LoadProductFriendlyNames",
                 new { controller = "HasAllProducts", action = "LoadProductFriendlyNames" }
            );

            //HasOneProduct
            routeBuilder.MapRoute("Plugin.DiscountRequirements.HasOneProduct.Configure",
                 "Admin/HasOneProduct/Configure",
                 new { controller = "HasOneProduct", action = "Configure" }
            );
            routeBuilder.MapRoute("Plugin.DiscountRequirements.HasOneProduct.ProductAddPopup",
                 "Admin/HasOneProduct/ProductAddPopup",
                 new { controller = "HasOneProduct", action = "ProductAddPopup" }
            );
            routeBuilder.MapRoute("Plugin.DiscountRequirements.HasOneProduct.ProductAddPopupList",
                 "Admin/HasOneProduct/ProductAddPopupList",
                 new { controller = "HasOneProduct", action = "ProductAddPopupList" }
            );
            routeBuilder.MapRoute("Plugin.DiscountRequirements.HasOneProduct.LoadProductFriendlyNames",
                 "Admin/HasOneProduct/LoadProductFriendlyNames",
                 new { controller = "HasOneProduct", action = "LoadProductFriendlyNames" }
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
