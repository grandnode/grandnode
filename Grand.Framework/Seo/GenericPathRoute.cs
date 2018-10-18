using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Localization;
using Grand.Core.Infrastructure;
using Grand.Framework.Localization;
using Grand.Services.Events;
using Grand.Services.Seo;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework.Seo
{
    /// <summary>
    /// Provides properties and methods for defining a SEO friendly route, and for getting information about the route.
    /// </summary>
    public class GenericPathRoute : LocalizedRoute
    {
        #region Fields

        private readonly IRouter _target;

        #endregion

        #region Ctor

        public GenericPathRoute(IRouter target, string routeName, string routeTemplate, RouteValueDictionary defaults,
            IDictionary<string, object> constraints, RouteValueDictionary dataTokens, IInlineConstraintResolver inlineConstraintResolver)
            : base(target, routeName, routeTemplate, defaults, constraints, dataTokens, inlineConstraintResolver)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get route values for current route
        /// </summary>
        /// <param name="context">Route context</param>
        /// <returns>Route values</returns>
        protected RouteValueDictionary GetRouteValues(RouteContext context)
        {

            var path = context.HttpContext.Request.Path.Value;
            if (this.SeoFriendlyUrlsForPathEnabled && !this.SeoFriendlyUrlsForLanguagesEnabled)
            {
                string lastpath = path.Split('/').Where(x => !string.IsNullOrEmpty(x)).LastOrDefault();
                path = $"/{lastpath}";
            }
            //remove language code from the path if it's localized URL
            if (this.SeoFriendlyUrlsForLanguagesEnabled && path.IsLocalizedUrl(context.HttpContext.Request.PathBase, false, out Language language))
                path = path.RemoveLanguageSeoCodeFromUrl(context.HttpContext.Request.PathBase, false);

            //parse route data
            var routeValues = new RouteValueDictionary(this.ParsedTemplate.Parameters
                .Where(parameter => parameter.DefaultValue != null)
                .ToDictionary(parameter => parameter.Name, parameter => parameter.DefaultValue));
            var matcher = new TemplateMatcher(this.ParsedTemplate, routeValues);
            matcher.TryMatch(path, routeValues);

            return routeValues;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Route request to the particular action
        /// </summary>
        /// <param name="context">A route context object</param>
        /// <returns>Task of the routing</returns>
        public override Task RouteAsync(RouteContext context)
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return Task.CompletedTask;

            //try to get slug from the route data
            var routeValues = GetRouteValues(context);
            if (!routeValues.TryGetValue("GenericSeName", out object slugValue) || string.IsNullOrEmpty(slugValue as string))
                return Task.CompletedTask;

            var slug = slugValue as string;

            //virtual directory path
            var pathBase = context.HttpContext.Request.PathBase;

            //performance optimization, we load a cached verion here. It reduces number of SQL requests for each page load
            var urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();
            var urlRecord = urlRecordService.GetBySlugCached(slug);

            //no URL record found
            if (urlRecord == null)
                return Task.CompletedTask;

            //if URL record is not active let's find the latest one
            if (!urlRecord.IsActive)
            {
                var activeSlug = urlRecordService.GetActiveSlug(urlRecord.EntityId, urlRecord.EntityName, urlRecord.LanguageId);
                if (string.IsNullOrEmpty(activeSlug))
                    return Task.CompletedTask;

                //redirect to active slug if found
                var redirectionRouteData = new RouteData(context.RouteData);
                redirectionRouteData.Values["controller"] = "Common";
                redirectionRouteData.Values["action"] = "InternalRedirect";
                redirectionRouteData.Values["url"] = $"{pathBase}/{activeSlug}{context.HttpContext.Request.QueryString}";
                redirectionRouteData.Values["permanentRedirect"] = true;
                context.HttpContext.Items["grand.RedirectFromGenericPathRoute"] = true;
                context.RouteData = redirectionRouteData;
                return _target.RouteAsync(context);
            }

            //ensure that the slug is the same for the current language, 
            //otherwise it can cause some issues when customers choose a new language but a slug stays the same
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var slugForCurrentLanguage = SeoExtensions.GetSeName(urlRecord.EntityId, urlRecord.EntityName, workContext.WorkingLanguage.Id);
            if (!string.IsNullOrEmpty(slugForCurrentLanguage) && !slugForCurrentLanguage.Equals(slug, StringComparison.OrdinalIgnoreCase))
            {
                //we should make validation above because some entities does not have SeName for standard (Id = 0) language (e.g. news, blog posts)

                //redirect to the page for current language
                var redirectionRouteData = new RouteData(context.RouteData);
                redirectionRouteData.Values["controller"] = "Common";
                redirectionRouteData.Values["action"] = "InternalRedirect";
                redirectionRouteData.Values["url"] = $"{pathBase}/{slugForCurrentLanguage}{context.HttpContext.Request.QueryString}";
                redirectionRouteData.Values["permanentRedirect"] = false;
                context.HttpContext.Items["grand.RedirectFromGenericPathRoute"] = true;
                context.RouteData = redirectionRouteData;
                return _target.RouteAsync(context);
            }

            //since we are here, all is ok with the slug, so process URL
            var currentRouteData = new RouteData(context.RouteData);
            switch (urlRecord.EntityName.ToLowerInvariant())
            {
                case "product":
                    currentRouteData.Values["controller"] = "Product";
                    currentRouteData.Values["action"] = "ProductDetails";
                    currentRouteData.Values["productid"] = urlRecord.EntityId;
                    currentRouteData.Values["SeName"] = urlRecord.Slug;
                    break;
                case "category":
                    currentRouteData.Values["controller"] = "Catalog";
                    currentRouteData.Values["action"] = "Category";
                    currentRouteData.Values["categoryid"] = urlRecord.EntityId;
                    currentRouteData.Values["SeName"] = urlRecord.Slug;
                    break;
                case "manufacturer":
                    currentRouteData.Values["controller"] = "Catalog";
                    currentRouteData.Values["action"] = "Manufacturer";
                    currentRouteData.Values["manufacturerid"] = urlRecord.EntityId;
                    currentRouteData.Values["SeName"] = urlRecord.Slug;
                    break;
                case "vendor":
                    currentRouteData.Values["controller"] = "Catalog";
                    currentRouteData.Values["action"] = "Vendor";
                    currentRouteData.Values["vendorid"] = urlRecord.EntityId;
                    currentRouteData.Values["SeName"] = urlRecord.Slug;
                    break;
                case "newsitem":
                    currentRouteData.Values["controller"] = "News";
                    currentRouteData.Values["action"] = "NewsItem";
                    currentRouteData.Values["newsItemId"] = urlRecord.EntityId;
                    currentRouteData.Values["SeName"] = urlRecord.Slug;
                    break;
                case "blogpost":
                    currentRouteData.Values["controller"] = "Blog";
                    currentRouteData.Values["action"] = "BlogPost";
                    currentRouteData.Values["blogPostId"] = urlRecord.EntityId;
                    currentRouteData.Values["SeName"] = urlRecord.Slug;
                    break;
                case "topic":
                    currentRouteData.Values["controller"] = "Topic";
                    currentRouteData.Values["action"] = "TopicDetails";
                    currentRouteData.Values["topicId"] = urlRecord.EntityId;
                    currentRouteData.Values["SeName"] = urlRecord.Slug;
                    break;
                case "knowledgebasearticle":
                    currentRouteData.Values["controller"] = "Knowledgebase";
                    currentRouteData.Values["action"] = "KnowledgebaseArticle";
                    currentRouteData.Values["articleId"] = urlRecord.EntityId;
                    currentRouteData.Values["SeName"] = urlRecord.Slug;
                    break;
                case "knowledgebasecategory":
                    currentRouteData.Values["controller"] = "Knowledgebase";
                    currentRouteData.Values["action"] = "ArticlesByCategory";
                    currentRouteData.Values["categoryId"] = urlRecord.EntityId;
                    currentRouteData.Values["SeName"] = urlRecord.Slug;
                    break;
                default:
                    //no record found, thus generate an event this way developers could insert their own types
                    EngineContext.Current.Resolve<IEventPublisher>().Publish(new CustomUrlRecordEntityNameRequested(currentRouteData, urlRecord));
                    break;
            }
            context.RouteData = currentRouteData;

            //route request
            return _target.RouteAsync(context);
        }

        #endregion
    }
}