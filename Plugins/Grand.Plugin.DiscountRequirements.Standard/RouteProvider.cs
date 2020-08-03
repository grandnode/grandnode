using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.DiscountRequirements
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            //CustomerRoles
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.CustomerRoles.Configure",
                 "Admin/CustomerRoles/Configure",
                 new { controller = "CustomerRoles", action = "Configure" }
            );

            //HadSpentAmount
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.HadSpentAmount.Configure",
                 "Admin/HadSpentAmount/Configure",
                 new { controller = "HadSpentAmount", action = "Configure" }
            );

            //HasAllProducts
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.HasAllProducts.Configure",
                 "Admin/HasAllProducts/Configure",
                 new { controller = "HasAllProducts", action = "Configure" }
            );
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.HasAllProducts.ProductAddPopup",
                 "Admin/HasAllProducts/ProductAddPopup",
                 new { controller = "HasAllProducts", action = "ProductAddPopup" }
            );
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.HasAllProducts.ProductAddPopupList",
                 "Admin/HasAllProducts/ProductAddPopupList",
                 new { controller = "HasAllProducts", action = "ProductAddPopupList" }
            );
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.HasAllProducts.LoadProductFriendlyNames",
                 "Admin/HasAllProducts/LoadProductFriendlyNames",
                 new { controller = "HasAllProducts", action = "LoadProductFriendlyNames" }
            );

            //HasOneProduct
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.HasOneProduct.Configure",
                 "Admin/HasOneProduct/Configure",
                 new { controller = "HasOneProduct", action = "Configure" }
            );
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.HasOneProduct.ProductAddPopup",
                 "Admin/HasOneProduct/ProductAddPopup",
                 new { controller = "HasOneProduct", action = "ProductAddPopup" }
            );
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.HasOneProduct.ProductAddPopupList",
                 "Admin/HasOneProduct/ProductAddPopupList",
                 new { controller = "HasOneProduct", action = "ProductAddPopupList" }
            );
            routeBuilder.MapControllerRoute("Plugin.DiscountRequirements.HasOneProduct.LoadProductFriendlyNames",
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
