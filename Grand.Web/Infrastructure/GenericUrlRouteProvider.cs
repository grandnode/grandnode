using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Web.Infrastructure
{
    public partial class GenericUrlRouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapDynamicControllerRoute<SlugRouteValueTransformer>("{SeName}");

            //and default one
            routeBuilder.MapControllerRoute(
                name: "Default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            ////generic URLs
            routeBuilder.MapControllerRoute(
                name: "GenericUrl",
                pattern: "{GenericSeName}",
                new { controller = "Common", action = "GenericUrl" }
                );

            ////define this routes to use in UI views (in case if you want to customize some of them later)
            routeBuilder.MapControllerRoute(
                name: "Product",
                pattern: "{SeName}",
                new { controller = "Product", action = "ProductDetails" }
                );

            routeBuilder.MapControllerRoute(
                name: "Category",
                pattern: "{SeName}",
                new { controller = "Catalog", action = "Category" }
                );

            routeBuilder.MapControllerRoute(
                name: "Manufacturer",
                pattern: "{SeName}",
                new { controller = "Catalog", action = "Manufacturer" }
                );

            routeBuilder.MapControllerRoute(
                name: "Vendor",
                pattern: "{SeName}",
                new { controller = "Catalog", action = "Vendor" }
                );

            routeBuilder.MapControllerRoute(
                name: "NewsItem",
                pattern: "{SeName}",
                new { controller = "News", action = "NewsItem" }
                );

            routeBuilder.MapControllerRoute(
                name: "BlogPost",
                pattern: "{SeName}",
                new { controller = "Blog", action = "BlogPost" }
                );

            routeBuilder.MapControllerRoute(
                name: "Topic",
                pattern: "{SeName}",
                new { controller = "Topic", action = "TopicDetails" }
                );

            routeBuilder.MapControllerRoute(
                name: "KnowledgebaseArticle",
                pattern: "{SeName}",
                new { controller = "Knowledgebase", action = "KnowledgebaseArticle" }
                );

            routeBuilder.MapControllerRoute(
                name: "KnowledgebaseCategory",
                pattern: "{SeName}",
                new { controller = "Knowledgebase", action = "ArticlesByCategory" }
                );

            routeBuilder.MapControllerRoute(
                name: "Course",
                pattern: "{SeName}",
                new { controller = "Course", action = "Details" }
                );
        }

        public int Priority {
            get {
                //it should be the last route
                //we do not set it to -int.MaxValue so it could be overridden (if required)
                return -1000000;
            }
        }
    }
}
