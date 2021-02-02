using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Domain.Vendors;
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
        private readonly VendorSettings _vendorSettings;
        private readonly IThemeProvider _themeProvider;

        private bool _themeIsCached, _adminthemeIsCached;
        private string _cachedThemeName, _cachedAdminThemeName;

        public ThemeContext(IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            StoreInformationSettings storeInformationSettings,
            VendorSettings vendorSettings,
            IThemeProvider themeProvider)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _storeInformationSettings = storeInformationSettings;
            _vendorSettings = vendorSettings;
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
                    theme = themeInstance.Name;
                }

                //cache theme
                _cachedThemeName = theme;
                _adminthemeIsCached = true;
                return theme;
            }
        }

        /// <summary>
        /// Get current theme system name
        /// </summary>
        public string AdminAreaThemeName {
            get {

                if (_adminthemeIsCached)
                    return _cachedAdminThemeName;

                var theme = "Default";

                if (!string.IsNullOrEmpty(_storeContext.CurrentStore.DefaultAdminTheme))
                    theme = _storeContext.CurrentStore.DefaultAdminTheme;

                if (_storeInformationSettings.AllowToSelectAdminTheme)
                {
                    if (_workContext.CurrentCustomer != null)
                    {
                        var customertheme = _workContext.CurrentCustomer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AdminThemeName, _storeContext.CurrentStore.Id);
                        if (!string.IsNullOrEmpty(customertheme))
                            theme = customertheme;
                    }
                }

                if (_workContext.CurrentVendor != null)
                {
                    if(!string.IsNullOrEmpty(_vendorSettings.DefaultAdminTheme))
                        theme = _vendorSettings.DefaultAdminTheme;
                }

                //cache theme
                _cachedAdminThemeName = theme;
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
