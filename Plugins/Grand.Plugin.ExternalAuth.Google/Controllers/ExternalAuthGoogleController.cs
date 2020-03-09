using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Plugin.ExternalAuth.Google.Models;
using Grand.Services.Authentication.External;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalAuth.Google.Controllers
{
    public class ExternalAuthGoogleController : BasePluginController
    {
        #region Fields

        private readonly GoogleExternalAuthSettings _googleExternalAuthSettings;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public ExternalAuthGoogleController(GoogleExternalAuthSettings googleExternalAuthSettings,
            IExternalAuthenticationService externalAuthenticationService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreService storeService,
            IWorkContext workContext
            )
        {
            _googleExternalAuthSettings = googleExternalAuthSettings;
            _externalAuthenticationService = externalAuthenticationService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                ClientKeyIdentifier = _googleExternalAuthSettings.ClientKeyIdentifier,
                ClientSecret = _googleExternalAuthSettings.ClientSecret
            };

            return View("~/Plugins/ExternalAuth.Google/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            _googleExternalAuthSettings.ClientKeyIdentifier = model.ClientKeyIdentifier;
            _googleExternalAuthSettings.ClientSecret = model.ClientSecret;
            await _settingService.SaveSetting(_googleExternalAuthSettings);
           
            //now clear settings cache
            await _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();

        }


        public IActionResult Login(string returnUrl)
        {
            if (!_externalAuthenticationService.ExternalAuthenticationMethodIsAvailable(GoogleAuthenticationDefaults.ProviderSystemName))
                throw new GrandException("Google authentication module cannot be loaded");

            if (string.IsNullOrEmpty(_googleExternalAuthSettings.ClientKeyIdentifier) || string.IsNullOrEmpty(_googleExternalAuthSettings.ClientSecret))
                throw new GrandException("Google authentication module not configured");

            //configure login callback action
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("LoginCallback", "ExternalAuthGoogle", new { returnUrl = returnUrl })
            };

            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
            //authenticate google user
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || !authenticateResult.Principal.Claims.Any())
                return RedirectToRoute("Login");

            //create external authentication parameters
            var authenticationParameters = new ExternalAuthenticationParameters
            {
                ProviderSystemName = GoogleAuthenticationDefaults.ProviderSystemName,
                AccessToken = await HttpContext.GetTokenAsync(GoogleDefaults.AuthenticationScheme, "access_token"),
                Email = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value,
                ExternalIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
                ExternalDisplayIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Name)?.Value,
                Claims = authenticateResult.Principal.Claims.Select(claim => new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList()
            };

            //authenticate grand user
            return await _externalAuthenticationService.Authenticate(authenticationParameters, returnUrl);
        }

        public IActionResult SignInFailed(string error_message)
        {
            //handle exception and display message to user
            var model = new FailedModel() {
                ErrorMessage = error_message
            };
            return View("~/Plugins/ExternalAuth.Google/Views/SignInFailed.cshtml", model);
        }
        #endregion
    }
}