using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework.Mvc.Routes;

namespace Grand.Web.Infrastructure
{
    //Routes used for backward compatibility with 1.x versions of nopCommerce
    public partial class BackwardCompatibility1XRouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            var supportPreviousNopcommerceVersions =
                !String.IsNullOrEmpty(ConfigurationManager.AppSettings["SupportPreviousNopcommerceVersions"]) &&
                Convert.ToBoolean(ConfigurationManager.AppSettings["SupportPreviousNopcommerceVersions"]);
            if (!supportPreviousNopcommerceVersions)
                return;

            //all old aspx URLs
            routes.MapRoute("", "{oldfilename}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "GeneralRedirect" },
                            new[] { "Grand.Web.Controllers" });
            
            //products
            routes.MapRoute("", "products/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectProduct"},
                            new[] { "Grand.Web.Controllers" });
            
            //categories
            routes.MapRoute("", "category/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectCategory" },
                            new[] { "Grand.Web.Controllers" });

            //manufacturers
            routes.MapRoute("", "manufacturer/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectManufacturer" },
                            new[] { "Grand.Web.Controllers" });

            //product tags
            routes.MapRoute("", "producttag/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectProductTag" },
                            new[] { "Grand.Web.Controllers" });

            //news
            routes.MapRoute("", "news/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectNewsItem" },
                            new[] { "Grand.Web.Controllers" });

            //blog posts
            routes.MapRoute("", "blog/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectBlogPost" },
                            new[] { "Grand.Web.Controllers" });

            //topics
            routes.MapRoute("", "topic/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectTopic" },
                            new[] { "Grand.Web.Controllers" });

            //forums
            routes.MapRoute("", "boards/fg/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectForumGroup" },
                            new[] { "Grand.Web.Controllers" });
            routes.MapRoute("", "boards/f/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectForum" },
                            new[] { "Grand.Web.Controllers" });
            routes.MapRoute("", "boards/t/{id}.aspx",
                            new { controller = "BackwardCompatibility1X", action = "RedirectForumTopic" },
                            new[] { "Grand.Web.Controllers" });
        }

        public int Priority
        {
            get
            {
                //register it after all other IRouteProvider are processed
                return -1000;
            }
        }
    }
}
