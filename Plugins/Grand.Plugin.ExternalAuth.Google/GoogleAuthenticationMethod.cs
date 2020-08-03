using Grand.Core;
using Grand.Core.Plugins;
using Grand.Services.Authentication.External;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalAuth.Google
{
    /// <summary>
    /// Represents method for the authentication with google account
    /// </summary>
    public class GoogleAuthenticationMethod : BasePlugin, IExternalAuthenticationMethod
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public GoogleAuthenticationMethod(ISettingService settingService, ILocalizationService localizationService, ILanguageService languageService,
            IWebHelper webHelper)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _languageService = languageService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ExternalAuthGoogle/Configure";
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "GoogleAuthentication";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override async Task Install()
        {
            //settings
            await _settingService.SaveSetting(new GoogleExternalAuthSettings());

            //locales
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.Description", "<h4>Google Developers Console</h4><ul><li>Go to the <a href=\"https://console.developers.google.com/\" target=\"_blank\">Google Developers Console</a> and log in with your Google Developer Account</li><li>Select \"Create Project\"</li><li>Go to APIs & Auth -> Credentials in the left-hand navigation panel</li><li>Select \"Create new Client ID\" in the OAuth Panel</li><li>In the creation panel:<ul><li>Select \"Web application\" as Application Type</li><li>Set \"Authorized JavaScript origins\" to the URL of your store (http://www.yourStore.com)</li><li>Set \"Authorized redirect URI\" to URL of login callback (http://www.yourStore.com/signin-google)</li></ul></li><li>Then go to APIs & Auth -> Consent Screen and fill out</li><li>Now get your API key (Client ID and Client Secret) and configure your store</li><li>Please remember to restart the application after changes.</li></ul><p>For more details, read the Google docs: <a href=\"https://developers.google.com/accounts/docs/OAuth2\">Using OAuth 2.0 to Access Google APIs</a>.</p>");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.Login", "Login using Google account");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.ClientKeyIdentifier", "Client ID");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.ClientKeyIdentifier.Hint", "Enter your Client ID key here. You can find it on Google Developers console page.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.ClientSecret", "Client Secret");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.ClientSecret.Hint", "Enter your client secret here. You can find it on your Google Developers console page.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.Title", "<h4>Configuring Google OAuth2</h4>");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Externalauth.Google.Failed", "Failed authentication");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Externalauth.Google.Failed.Errormessage", "Error message: ");

            await base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override async Task Uninstall()
        {
            //settings
            await _settingService.DeleteSetting<GoogleExternalAuthSettings>();

            //locales
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.Description");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.Login");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.ClientKeyIdentifier");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.ClientKeyIdentifier.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.ClientSecret");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.ClientSecret.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.ExternalAuth.Google.Title");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Externalauth.Google.Failed");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Externalauth.Google.Failed.Errormessage");

            await base.Uninstall();
        }

        #endregion
    }
}
