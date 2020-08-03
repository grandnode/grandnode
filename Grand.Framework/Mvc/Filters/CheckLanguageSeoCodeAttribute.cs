using Grand.Core;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Domain.Localization;
using Grand.Framework.Localization;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
            private readonly GrandConfig _config;

            #endregion

            #region Ctor

            public CheckLanguageSeoCodeFilter(IWebHelper webHelper,
                IWorkContext workContext, ILanguageService languageService,
                GrandConfig config)
            {
                _webHelper = webHelper;
                _workContext = workContext;
                _languageService = languageService;
                _config = config;
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
                if (!_config.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    await next();
                    return;
                }

                var lang = context.RouteData.Values["language"];
                if (lang == null)
                {
                    await next();
                    return;
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