using Grand.Core.Configuration;
using Grand.Services.Localization;
using Grand.Services.Seo;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework.Mvc.Routing
{
    public class SlugRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly GrandConfig _config;

        public SlugRouteTransformer(
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            GrandConfig config)
        {
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _config = config;
        }

        protected async ValueTask<string> GetSeName(string entityId, string entityName, string languageId)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(languageId))
            {
                result = await _urlRecordService.GetActiveSlug(entityId, entityName, languageId);
            }
            //set default value if required
            if (string.IsNullOrEmpty(result))
            {
                result = await _urlRecordService.GetActiveSlug(entityId, entityName, "");
            }

            return result;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext context, RouteValueDictionary values)
        {
            if (values == null)
                return null;

            var slug = values["SeName"];
            if (slug == null)
                return values;

            var urlRecord = await _urlRecordService.GetBySlugCached(slug.ToString());

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
                context.Items["grand.RedirectFromGenericPathRoute"] = true;
                return values;
            }

            //ensure that the slug is the same for the current language, 
            //otherwise it can cause some issues when customers choose a new language but a slug stays the same
            if (_config.SeoFriendlyUrlsForLanguagesEnabled)
            {
                var urllanguage = values["language"];
                if (urllanguage != null && !string.IsNullOrEmpty(urllanguage.ToString()))
                {
                    var language = (await _languageService.GetAllLanguages()).FirstOrDefault(x => x.UniqueSeoCode.ToLowerInvariant() == urllanguage.ToString().ToLowerInvariant());
                    if (language == null)
                        language = (await _languageService.GetAllLanguages()).FirstOrDefault();

                    var slugForCurrentLanguage = await GetSeName(urlRecord.EntityId, urlRecord.EntityName, language.Id);
                    if (!string.IsNullOrEmpty(slugForCurrentLanguage) && !slugForCurrentLanguage.Equals(slug.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        values["controller"] = "Common";
                        values["action"] = "InternalRedirect";
                        values["url"] = $"{context.Request.PathBase}/{slugForCurrentLanguage}{context.Request.QueryString}";
                        values["permanentRedirect"] = false;
                        context.Items["grand.RedirectFromGenericPathRoute"] = true;
                        return values;
                    }
                }
            }
            else
            {
                //TODO - redirect when current lang is not the same as slug lang
            }

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
                    await context.RequestServices.GetRequiredService<IMediator>().Publish(new CustomUrlRecordEntityName(values, urlRecord));
                    break;
            }
            return values;
        }
    }
}
