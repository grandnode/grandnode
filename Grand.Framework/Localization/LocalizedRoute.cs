using Grand.Core.Data;
using Grand.Core.Domain.Localization;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Framework.Localization
{
    /// <summary>
    /// Provides properties and methods for defining a localized route, and for getting information about the localized route.
    /// </summary>
    public class LocalizedRoute : Route
    {
        #region Fields

        private readonly IRouter _target;
        private bool? _seoFriendlyUrlsForLanguagesEnabled;
        private bool? _seoFriendlyUrlsForPathEnabled;
        private IList<Language> _languages;

        #endregion

        #region Ctor

        public LocalizedRoute(IRouter target, string routeName, string routeTemplate, RouteValueDictionary defaults,
            IDictionary<string, object> constraints, RouteValueDictionary dataTokens, IInlineConstraintResolver inlineConstraintResolver)
            : base(target, routeName, routeTemplate, defaults, constraints, dataTokens, inlineConstraintResolver)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns information about the URL that is associated with the route
        /// </summary>
        /// <param name="context">A context for virtual path generation operations</param>
        /// <returns>Information about the route and virtual path</returns>
        public override VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            //get base virtual path
            var data = base.GetVirtualPath(context);
            if (data == null)
                return null;

            if (!DataSettingsHelper.DatabaseIsInstalled() || !SeoFriendlyUrlsForLanguagesEnabled(context.HttpContext))
                return data;

            //add language code to page URL in case if it's localized URL
            var path = context.HttpContext.Request.Path.Value;
            if (path.IsLocalizedUrl(context.HttpContext.Request.PathBase, false, Languages(context.HttpContext), out Language language))
                data.VirtualPath = $"/{language.UniqueSeoCode}{data.VirtualPath}";

            return data;
        }

        /// <summary>
        /// Route request to the particular action
        /// </summary>
        /// <param name="context">A route context object</param>
        /// <returns>Task of the routing</returns>
        public override async Task RouteAsync(RouteContext context)
        {
            if (!DataSettingsHelper.DatabaseIsInstalled() || !SeoFriendlyUrlsForLanguagesEnabled(context.HttpContext))
            {
                await base.RouteAsync(context);
                return;
            }

            await PrepareLanguages(context.HttpContext);

            //if path isn't localized, no special action required
            var path = context.HttpContext.Request.Path.Value;
            if (!path.IsLocalizedUrl(context.HttpContext.Request.PathBase, false, Languages(context.HttpContext), out Language language))
            {
                await base.RouteAsync(context);
                return;
            }

            //remove language code and application path from the path
            var newPath = path.RemoveLanguageSeoCodeFromUrl(context.HttpContext.Request.PathBase, false);

            //set new request path and try to get route handler
            context.HttpContext.Request.Path = newPath;
            await base.RouteAsync(context);

        }

        /// <summary>
        /// Clear _seoFriendlyUrlsForLanguagesEnabled cached value
        /// </summary>
        public virtual void ClearSeoFriendlyUrlsCachedValue()
        {
            _seoFriendlyUrlsForLanguagesEnabled = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets value of _seoFriendlyUrlsForLanguagesEnabled settings
        /// </summary>
        protected bool SeoFriendlyUrlsForLanguagesEnabled(HttpContext httpContext)
        {
            if (!_seoFriendlyUrlsForLanguagesEnabled.HasValue)
                _seoFriendlyUrlsForLanguagesEnabled = httpContext.RequestServices.GetRequiredService<LocalizationSettings>().SeoFriendlyUrlsForLanguagesEnabled;

            return _seoFriendlyUrlsForLanguagesEnabled.Value;
        }

        /// <summary>
        /// Gets value of _seoFriendlyUrlsForPathEnabled settings
        /// </summary>
        protected bool SeoFriendlyUrlsForPathEnabled(HttpContext httpContext)
        {
            if (!_seoFriendlyUrlsForPathEnabled.HasValue)
                _seoFriendlyUrlsForPathEnabled = httpContext.RequestServices.GetRequiredService<LocalizationSettings>().SeoFriendlyUrlsForPathEnabled;

            return _seoFriendlyUrlsForPathEnabled.Value;
        }

        protected async Task PrepareLanguages(HttpContext httpContext)
        {
            if (_languages == null)
            {
                var languageService = httpContext.RequestServices.GetRequiredService<ILanguageService>();
                _languages = await languageService.GetAllLanguages();
            }
        }

        /// <summary>
        /// Gets all languges
        /// </summary>
        protected IList<Language> Languages(HttpContext context)
        {
            if (_languages == null)
            {
                PrepareLanguages(context).GetAwaiter().GetResult();
            }
            return _languages;
        }

        #endregion
    }
}