using Grand.Framework.Localization;
using Grand.Framework.Mvc.Routing;
using Grand.Framework.Seo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Web.Infrastructure
{
    public partial class GenericUrlRouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //and default one
            routeBuilder.MapRoute("Default", "{controller}/{action}/{id?}");

            //generic URLs
            routeBuilder.MapGenericPathRoute("GenericUrl", "{GenericSeName}", new { controller = "Common", action = "GenericUrl" });

            //define this routes to use in UI views (in case if you want to customize some of them later)
            routeBuilder.MapLocalizedRoute("Product", "{SeName}", new { controller = "Product", action = "ProductDetails" });

            routeBuilder.MapLocalizedRoute("Category", "{SeName}", new { controller = "Catalog", action = "Category" });

            routeBuilder.MapLocalizedRoute("Manufacturer", "{SeName}", new { controller = "Catalog", action = "Manufacturer" });

            routeBuilder.MapLocalizedRoute("Vendor", "{SeName}", new { controller = "Catalog", action = "Vendor" });

            routeBuilder.MapLocalizedRoute("NewsItem", "{SeName}", new { controller = "News", action = "NewsItem" });

            routeBuilder.MapLocalizedRoute("BlogPost", "{SeName}", new { controller = "Blog", action = "BlogPost" });

            routeBuilder.MapLocalizedRoute("Topic", "{SeName}", new { controller = "Topic", action = "TopicDetails" });

            routeBuilder.MapLocalizedRoute("KnowledgebaseArticle", "{SeName}", new { controller = "Knowledgebase", action = "KnowledgebaseArticle" });

            routeBuilder.MapLocalizedRoute("KnowledgebaseCategory", "{SeName}", new { controller = "Knowledgebase", action = "ArticlesByCategory" });
        }

        public int Priority
        {
            get
            {
                //it should be the last route
                //we do not set it to -int.MaxValue so it could be overridden (if required)
                return -1000000;
            }
        }
    }
}
