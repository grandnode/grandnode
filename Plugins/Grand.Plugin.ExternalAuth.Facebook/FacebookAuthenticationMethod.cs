using Grand.Core;
using Grand.Core.Plugins;
using Grand.Services.Authentication.External;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalAuth.Facebook
{
    /// <summary>
    /// Represents method for the authentication with Facebook account
    /// </summary>
    public class FacebookAuthenticationMethod : BasePlugin, IExternalAuthenticationMethod
    {
        #region Fields
        
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public FacebookAuthenticationMethod(ISettingService settingService,
            ILocalizationService localizationService, 
            ILanguageService languageService,
            IWebHelper webHelper)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/FacebookAuthentication/Configure";
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "FacebookAuthentication";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override async Task Install()
        {
            //settings
            await _settingService.SaveSetting(new FacebookExternalAuthSettings());

            //locales
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.Login", "Login using Facebook account");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.ClientKeyIdentifier", "App ID/API Key");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.ClientKeyIdentifier.Hint", "Enter your app ID/API key here. You can find it on your FaceBook application page.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.ClientSecret", "App Secret");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.ClientSecret.Hint", "Enter your app secret here. You can find it on your FaceBook application page.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.Failed", "Facebook - Login error");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.Failed.ErrorCode", "Error code");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.Failed.ErrorMessage", "Error message");

            await base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override async Task Uninstall()
        {
            //settings
            await _settingService.DeleteSetting<FacebookExternalAuthSettings>();

            //locales
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.Login");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.ClientKeyIdentifier");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.ClientKeyIdentifier.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.ClientSecret");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Facebook.ClientSecret.Hint");

            await base.Uninstall();
        }

        #endregion
    }
}
