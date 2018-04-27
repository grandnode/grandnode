using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Localization;
using Grand.Services.Localization;
using Grand.Framework.Localization;
using System.Linq;
using Microsoft.AspNetCore.Http;

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
        private class CheckLanguageSeoCodeFilter : IActionFilter
        {
            #region Fields

            private readonly ILanguageService _languageService;
            private readonly IWebHelper _webHelper;
            private readonly IWorkContext _workContext;
            private readonly LocalizationSettings _localizationSettings;

            #endregion

            #region Ctor

            public CheckLanguageSeoCodeFilter(ILanguageService languageService,
                IWebHelper webHelper,
                IWorkContext workContext,
                LocalizationSettings localizationSettings)
            {
                this._languageService = languageService;
                this._webHelper = webHelper;
                this._workContext = workContext;
                this._localizationSettings = localizationSettings;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null || context.HttpContext == null || context.HttpContext.Request == null)
                    return;

                if (!DataSettingsHelper.DatabaseIsInstalled())
                    return;

                //only in GET requests
                if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
                    return;

                //whether SEO friendly URLs are enabled
                if (!_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                    return;


                //ensure that this route is registered and localizable (LocalizedRoute in RouteProvider)
                if (context.RouteData == null
                    || context.RouteData.Routers == null
                    || !context.RouteData.Routers.ToList().Any(r => r is LocalizedRoute))
                    return;

                //check whether current page URL is already localized URL
                var pageUrl = _webHelper.GetRawUrl(context.HttpContext.Request);
                if (pageUrl.IsLocalizedUrl(context.HttpContext.Request.PathBase, true, out Language language))
                    return;

                //not localized yet, so redirect to the page with working language SEO code
                pageUrl = pageUrl.AddLanguageSeoCodeToUrl(context.HttpContext.Request.PathBase, true, _workContext.WorkingLanguage);
                context.Result = new RedirectResult(pageUrl, false);
            }

            /// <summary>
            /// Called after the action executes, before the action result
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuted(ActionExecutedContext context)
            {
                //do nothing
            }

            #endregion
        }

        #endregion
    }
}