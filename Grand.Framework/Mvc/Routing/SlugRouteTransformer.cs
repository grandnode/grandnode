using Grand.Services.Seo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace Grand.Framework.Mvc.Routing
{
    public class SlugRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly IUrlRecordService _urlRecordService;

        public SlugRouteTransformer(IUrlRecordService urlRecordService)
        {
            _urlRecordService = urlRecordService;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext context, RouteValueDictionary values)
        {
            var requestPath = context.Request.Path.Value;
            if (!string.IsNullOrEmpty(requestPath) && requestPath[0] == '/')
            {
                // Trim the leading slash
                requestPath = requestPath.Substring(1);
            }

            //performance optimization, we load a cached verion here. It reduces number of SQL requests for each page load
            var urlRecord = await _urlRecordService.GetBySlugCached(requestPath);

            //no URL record found
            if (urlRecord == null)
                return null;

            //if URL record is not active let's find the latest one
            if (!urlRecord.IsActive)
            {
                var activeSlug = await _urlRecordService.GetActiveSlug(urlRecord.EntityId, urlRecord.EntityName, urlRecord.LanguageId);
                if (string.IsNullOrEmpty(activeSlug))
                    return null;

                values["controller"] = "Common";
                values["action"] = "InternalRedirect";
                values["url"] = $"{context.Request.PathBase}/{activeSlug}{context.Request.QueryString}";
                values["permanentRedirect"] = true;
                values["grand.RedirectFromGenericPathRoute"] = true;
                return values;
            }

            //ensure that the slug is the same for the current language, 
            //otherwise it can cause some issues when customers choose a new language but a slug stays the same
            //var workContext = context.HttpContext.RequestServices.GetRequiredService<IWorkContext>();
            //var slugForCurrentLanguage = await SeoExtensions.GetSeName(urlRecordService, context.HttpContext, urlRecord.EntityId, urlRecord.EntityName, workContext.WorkingLanguage.Id);
            //if (!string.IsNullOrEmpty(slugForCurrentLanguage) && !slugForCurrentLanguage.Equals(slug, StringComparison.OrdinalIgnoreCase))
            //{
            //    //we should make validation above because some entities does not have SeName for standard (Id = 0) language (e.g. news, blog posts)

            //    //redirect to the page for current language
            //    var redirectionRouteData = new RouteData(context.RouteData);
            //    redirectionRouteData.Values["controller"] = "Common";
            //    redirectionRouteData.Values["action"] = "InternalRedirect";
            //    redirectionRouteData.Values["url"] = $"{pathBase}/{slugForCurrentLanguage}{context.HttpContext.Request.QueryString}";
            //    redirectionRouteData.Values["permanentRedirect"] = false;
            //    context.HttpContext.Items["grand.RedirectFromGenericPathRoute"] = true;
            //    context.RouteData = redirectionRouteData;
            //    await _target.RouteAsync(context);
            //    return;
            //}

            //since we are here, all is ok with the slug, so process URL
            //var currentRouteData = new RouteData(context.RouteData);
            switch (urlRecord.EntityName.ToLowerInvariant())
            {
                case "product":
                    values["controller"] = "Product";
                    values["action"] = "ProductDetails";
                    values["productid"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                case "category":
                    values["controller"] = "Catalog";
                    values["action"] = "Category";
                    values["categoryid"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                case "manufacturer":
                    values["controller"] = "Catalog";
                    values["action"] = "Manufacturer";
                    values["manufacturerid"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                case "vendor":
                    values["controller"] = "Catalog";
                    values["action"] = "Vendor";
                    values["vendorid"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                case "newsitem":
                    values["controller"] = "News";
                    values["action"] = "NewsItem";
                    values["newsItemId"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                case "blogpost":
                    values["controller"] = "Blog";
                    values["action"] = "BlogPost";
                    values["blogPostId"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                case "topic":
                    values["controller"] = "Topic";
                    values["action"] = "TopicDetails";
                    values["topicId"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                case "knowledgebasearticle":
                    values["controller"] = "Knowledgebase";
                    values["action"] = "KnowledgebaseArticle";
                    values["articleId"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                case "knowledgebasecategory":
                    values["controller"] = "Knowledgebase";
                    values["action"] = "ArticlesByCategory";
                    values["categoryId"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                case "course":
                    values["controller"] = "Course";
                    values["action"] = "Details";
                    values["courseId"] = urlRecord.EntityId;
                    values["SeName"] = urlRecord.Slug;
                    break;
                default:
                    //no record found, thus generate an event this way developers could insert their own types
                    //await context.HttpContext.RequestServices.GetRequiredService<IMediator>().Publish(new CustomUrlRecordEntityNameRequested(currentRouteData, urlRecord));
                    break;
            }
            //context.RouteData = currentRouteData;
            return values;
        }
    }
}
