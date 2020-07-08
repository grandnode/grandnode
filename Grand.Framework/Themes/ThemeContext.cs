using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Services.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework.Themes
{
    /// <summary>
    /// Theme context
    /// </summary>
    public partial class ThemeContext : IThemeContext
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IThemeProvider _themeProvider;

        private bool _themeIsCached;
        private string _cachedThemeName;

        public ThemeContext(IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            StoreInformationSettings storeInformationSettings,
            IThemeProvider themeProvider)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _storeInformationSettings = storeInformationSettings;
            _themeProvider = themeProvider;
        }

        /// <summary>
        /// Get current theme system name
        /// </summary>
        public string WorkingThemeName {
            get {
                if (_themeIsCached)
                    return _cachedThemeName;

                string theme = "";
                if (_storeInformationSettings.AllowCustomerToSelectTheme)
                {
                    if (_workContext.CurrentCustomer != null)
                        theme = _workContext.CurrentCustomer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.WorkingThemeName, _storeContext.CurrentStore.Id);
                }

                //default store theme
                if (string.IsNullOrEmpty(theme))
                    theme = _storeInformationSettings.DefaultStoreTheme;

                //ensure that theme exists
                if (!_themeProvider.ThemeConfigurationExists(theme))
                {
                    var themeInstance = _themeProvider.GetThemeConfigurations()
                        .FirstOrDefault();
                    if (themeInstance == null)
                        throw new Exception("No theme could be loaded");
                    theme = themeInstance.ThemeName;
                }

                //cache theme
                _cachedThemeName = theme;
                _themeIsCached = true;
                return theme;
            }
        }
        /// <summary>
        /// Set current theme system name
        /// </summary>
        /// <param name="themeName"></param>
        /// <returns></returns>
        public virtual async Task SetWorkingTheme(string themeName)
        {
            if (!_storeInformationSettings.AllowCustomerToSelectTheme)
                return;

            if (_workContext.CurrentCustomer == null)
                return;

            await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.WorkingThemeName, themeName, _storeContext.CurrentStore.Id);

            //clear cache
            _themeIsCached = false;
        }
    }
}
