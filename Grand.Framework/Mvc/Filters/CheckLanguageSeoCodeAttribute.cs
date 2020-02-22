﻿using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Localization;
using Grand.Framework.Localization;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents filter attribute that checks SEO friendly URLs for multiple languages and properly redirect if necessary
    /// </summary>
    public class CheckLanguageSeoCodeAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public CheckLanguageSeoCodeAttribute() : base(typeof(CheckLanguageSeoCodeFilter))
        {
        }

        #region Nested filter

        /// <summary>
        /// Represents a filter that checks SEO friendly URLs for multiple languages and properly redirect if necessary
        /// </summary>
        private class CheckLanguageSeoCodeFilter : IAsyncActionFilter
        {
            #region Fields

            private readonly IWebHelper _webHelper;
            private readonly IWorkContext _workContext;
            private readonly ILanguageService _languageService;
            private readonly LocalizationSettings _localizationSettings;

            #endregion

            #region Ctor

            public CheckLanguageSeoCodeFilter(IWebHelper webHelper,
                IWorkContext workContext, ILanguageService languageService,
                LocalizationSettings localizationSettings)
            {
                _webHelper = webHelper;
                _workContext = workContext;
                _languageService = languageService;
                _localizationSettings = localizationSettings;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context == null || context.HttpContext == null || context.HttpContext.Request == null)
                {
                    await next();
                    return;
                }

                if (!DataSettingsHelper.DatabaseIsInstalled())
                {
                    await next();
                    return;
                }

                //only in GET requests
                if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
                {
                    await next();
                    return;
                }

                //whether SEO friendly URLs are enabled
                if (!_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    await next();
                    return;
                }


                //ensure that this route is registered and localizable (LocalizedRoute in RouteProvider)
                //if (context.RouteData == null
                //    || context.RouteData.Routers == null
                //    || !context.RouteData.Routers.ToList().Any(r => r is LocalizedRoute))
                //{
                //    await next();
                //    return;
                //}
                var endpointFeature = context.HttpContext.Features[typeof(IEndpointFeature)] as IEndpointFeature;
                var endpoint = endpointFeature?.Endpoint;

                //note: endpoint will be null, if there was no resolved route
                if (endpoint != null)
                {
                    var routePattern = (endpoint as RouteEndpoint)?.RoutePattern
                                                                  ?.RawText;
                    //Console.WriteLine("Name: " + endpoint.DisplayName);
                    //Console.WriteLine($"Route Pattern: {routePattern}");
                    //Console.WriteLine("Metadata Types: " + string.Join(", ", endpoint.Metadata));
                }
                //check whether current page URL is already localized URL
                var pageUrl = _webHelper.GetRawUrl(context.HttpContext.Request);
                if (await (pageUrl.IsLocalizedUrlAsync(_languageService, context.HttpContext.Request.PathBase, true)))
                {
                    await next();
                    return;
                }

                //not localized yet, so redirect to the page with working language SEO code
                pageUrl = pageUrl.AddLanguageSeoCodeToUrl(context.HttpContext.Request.PathBase, true, _workContext.WorkingLanguage);
                context.Result = new RedirectResult(pageUrl, false);
            }

            #endregion
        }

        #endregion
    }
}