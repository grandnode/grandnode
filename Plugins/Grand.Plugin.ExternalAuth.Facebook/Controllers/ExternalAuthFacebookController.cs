﻿using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Plugins;
using Grand.Plugin.ExternalAuth.Facebook.Core;
using Grand.Plugin.ExternalAuth.Facebook.Models;
using Grand.Services.Authentication.External;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Framework;
using Grand.Web.Framework.Controllers;
using System;

namespace Grand.Plugin.ExternalAuth.Facebook.Controllers
{
    public class ExternalAuthFacebookController : BasePluginController
    {
        private readonly ISettingService _settingService;
        private readonly IOAuthProviderFacebookAuthorizer _oAuthProviderFacebookAuthorizer;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILocalizationService _localizationService;

        public ExternalAuthFacebookController(ISettingService settingService,
            IOAuthProviderFacebookAuthorizer oAuthProviderFacebookAuthorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWorkContext workContext,
            IPluginFinder pluginFinder,
            ILocalizationService localizationService)
        {
            this._settingService = settingService;
            this._oAuthProviderFacebookAuthorizer = oAuthProviderFacebookAuthorizer;
            this._openAuthenticationService = openAuthenticationService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._permissionService = permissionService;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._workContext = workContext;
            this._pluginFinder = pluginFinder;
            this._localizationService = localizationService;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return Content("Access denied");

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var facebookExternalAuthSettings = _settingService.LoadSetting<FacebookExternalAuthSettings>(storeScope);

            var model = new ConfigurationModel();
            model.ClientKeyIdentifier = facebookExternalAuthSettings.ClientKeyIdentifier;
            model.ClientSecret = facebookExternalAuthSettings.ClientSecret;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.ClientKeyIdentifier_OverrideForStore = _settingService.SettingExists(facebookExternalAuthSettings, x => x.ClientKeyIdentifier, storeScope);
                model.ClientSecret_OverrideForStore = _settingService.SettingExists(facebookExternalAuthSettings, x => x.ClientSecret, storeScope);
            }

            return View("~/Plugins/ExternalAuth.Facebook/Views/ExternalAuthFacebook/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return Content("Access denied");

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var facebookExternalAuthSettings = _settingService.LoadSetting<FacebookExternalAuthSettings>(storeScope);

            //save settings
            facebookExternalAuthSettings.ClientKeyIdentifier = model.ClientKeyIdentifier;
            facebookExternalAuthSettings.ClientSecret = model.ClientSecret;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.ClientKeyIdentifier_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(facebookExternalAuthSettings, x => x.ClientKeyIdentifier, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(facebookExternalAuthSettings, x => x.ClientKeyIdentifier, storeScope);

            if (model.ClientSecret_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(facebookExternalAuthSettings, x => x.ClientSecret, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(facebookExternalAuthSettings, x => x.ClientSecret, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo()
        {
            return View("~/Plugins/ExternalAuth.Facebook/Views/ExternalAuthFacebook/PublicInfo.cshtml");
        }

        [NonAction]
        private ActionResult LoginInternal(string returnUrl, bool verifyResponse)
        {
            var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName("ExternalAuth.Facebook");
            if (processor == null ||
                !processor.IsMethodActive(_externalAuthenticationSettings) ||
                !processor.PluginDescriptor.Installed ||
                !_pluginFinder.AuthenticateStore(processor.PluginDescriptor, _storeContext.CurrentStore.Id))
                throw new NopException("Facebook module cannot be loaded");

            var viewModel = new LoginModel();
            TryUpdateModel(viewModel);

            var result = _oAuthProviderFacebookAuthorizer.Authorize(returnUrl, verifyResponse);
            switch (result.AuthenticationStatus)
            {
                case OpenAuthenticationStatus.Error:
                    {
                        if (!result.Success)
                            foreach (var error in result.Errors)
                                ExternalAuthorizerHelper.AddErrorsToDisplay(error);

                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AssociateOnLogon:
                    {
                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AutoRegisteredEmailValidation:
                    {
                        //result
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation });
                    }
                case OpenAuthenticationStatus.AutoRegisteredAdminApproval:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval });
                    }
                case OpenAuthenticationStatus.AutoRegisteredStandard:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Standard });
                    }
                default:
                    break;
            }

            if (result.Result != null) return result.Result;
            return HttpContext.Request.IsAuthenticated ? new RedirectResult(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "~/") : new RedirectResult(Url.LogOn(returnUrl));
        }
        
        public ActionResult Login(string returnUrl)
        {
            return LoginInternal(returnUrl, false);
        }

        public ActionResult LoginCallback(string returnUrl)
        {
            return LoginInternal(returnUrl, true);
        }
    }
}