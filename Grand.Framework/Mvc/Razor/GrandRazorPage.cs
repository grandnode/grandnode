using Grand.Core;
using Grand.Core.Infrastructure;
using Grand.Services.Localization;
using Grand.Framework.Localization;
using Grand.Framework.Themes;

namespace Grand.Framework.Mvc.Razor
{
    /// <summary>
    /// Web view page
    /// </summary>
    /// <typeparam name="TModel">Model</typeparam>
    public abstract class GrandRazorPage<TModel> : Microsoft.AspNetCore.Mvc.Razor.RazorPage<TModel>
    {
        private ILocalizationService _localizationService;
        private Localizer _localizer;
        private IWorkContext _workContext;

        public IWorkContext WorkContext
        {
            get
            {
                if (_workContext == null)
                    _workContext = EngineContext.Current.Resolve<IWorkContext>();
                return _workContext;
            }
        }

        /// <summary>
        /// Get a localized resources
        /// </summary>
        public Localizer T
        {
            get
            {
                if (_localizationService == null)
                    _localizationService = EngineContext.Current.Resolve<ILocalizationService>();

                if (_localizer == null)
                {
                    _localizer = (format, args) =>
                    {
                        var resFormat = _localizationService.GetResource(format);
                        if (string.IsNullOrEmpty(resFormat))
                        {
                            return new LocalizedString(format);
                        }
                        return new LocalizedString((args == null || args.Length == 0)
                            ? resFormat
                            : string.Format(resFormat, args));
                    };
                }
                return _localizer;
            }
        }

        /// <summary>
        /// Gets a selected tab index (used in admin area to store selected tab index)
        /// </summary>
        /// <returns>Index</returns>
        public int GetSelectedTabIndex()
        {
            //keep this method synchornized with
            //"SetSelectedTabIndex" method of \Administration\Controllers\BaseGrandController.cs
            int index = 0;
            string dataKey = "Grand.selected-tab-index";
            if (ViewData[dataKey] is int)
            {
                index = (int)ViewData[dataKey];
            }
            if (TempData[dataKey] is int)
            {
                index = (int)TempData[dataKey];
            }

            //ensure it's not negative
            if (index < 0)
                index = 0;

            return index;
        }

        /// <summary>
        /// Return a value indicating whether the working language and theme support RTL (right-to-left)
        /// </summary>
        /// <returns></returns>
        public bool ShouldUseRtlTheme()
        {
            var supportRtl = WorkContext.WorkingLanguage.Rtl;
            if (supportRtl)
            {
                //ensure that the active theme also supports it
                var themeProvider = EngineContext.Current.Resolve<IThemeProvider>();
                var themeContext = EngineContext.Current.Resolve<IThemeContext>();
                supportRtl = themeProvider.GetThemeConfiguration(themeContext.WorkingThemeName).SupportRtl;
            }
            return supportRtl;
        }
        public string WorkingLanguage()
        {
            return WorkContext.WorkingLanguage.UniqueSeoCode;
        }

    }

    /// <summary>
    /// Web view page
    /// </summary>
    public abstract class GrandRazorPage : GrandRazorPage<dynamic>
    {
    }
}