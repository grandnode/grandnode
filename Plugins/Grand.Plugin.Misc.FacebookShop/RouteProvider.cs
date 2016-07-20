using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework.Mvc.Routes;

namespace Grand.Plugin.Misc.FacebookShop
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //home page
            routes.MapRoute("Plugin.Misc.FacebookShop.Index",
                 "facebook/shop/",
                 new { controller = "MiscFacebookShop", action = "Index" },
                 new[] { "Grand.Plugin.Misc.FacebookShop.Controllers" }
            );

            //category page
            routes.MapRoute("Plugin.Misc.FacebookShop.Category",
                 "facebook/shop/category/{categoryId}/",
                 new { controller = "MiscFacebookShop", action = "Category" },
                 new { categoryId = @"\d+" },
                 new[] { "Grand.Plugin.Misc.FacebookShop.Controllers" }
            );

            //search page
            routes.MapRoute("Plugin.Misc.FacebookShop.ProductSearch",
                 "facebook/shop/search/",
                 new { controller = "MiscFacebookShop", action = "Search" },
                 new[] { "Grand.Plugin.Misc.FacebookShop.Controllers" }
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
