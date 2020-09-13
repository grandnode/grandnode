using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Plugin.Widgets.FacebookPixel.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.Widgets.FacebookPixel.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    [PermissionAuthorize(PermissionSystemName.Widgets)]
    public class WidgetsFacebookPixelController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public WidgetsFacebookPixelController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _localizationService = localizationService;
        }

        [AuthorizeAdmin]
        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var facebookPixelSettings = _settingService.LoadSetting<FacebookPixelSettings>(storeScope);
            var model = new ConfigurationModel();
            model.PixelId = facebookPixelSettings.PixelId;
            model.PixelScript = facebookPixelSettings.PixelScript;
            model.AddToCartScript = facebookPixelSettings.AddToCartScript;
            model.DetailsOrderScript = facebookPixelSettings.DetailsOrderScript;
            model.AllowToDisableConsentCookie = facebookPixelSettings.AllowToDisableConsentCookie;
            model.ConsentName = facebookPixelSettings.ConsentName;
            model.ConsentDescription = facebookPixelSettings.ConsentDescription;
            model.ConsentDefaultState = facebookPixelSettings.ConsentDefaultState;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (!string.IsNullOrEmpty(storeScope))
            {
                model.PixelId_OverrideForStore = _settingService.SettingExists(facebookPixelSettings, x => x.PixelId, storeScope);
                model.PixelScript_OverrideForStore = _settingService.SettingExists(facebookPixelSettings, x => x.PixelScript, storeScope);
                model.AddToCartScript_OverrideForStore = _settingService.SettingExists(facebookPixelSettings, x => x.AddToCartScript, storeScope);
                model.DetailsOrderScript_OverrideForStore = _settingService.SettingExists(facebookPixelSettings, x => x.DetailsOrderScript, storeScope);
                model.AllowToDisableConsentCookie_OverrideForStore = _settingService.SettingExists(facebookPixelSettings, x => x.AllowToDisableConsentCookie, storeScope);
                model.ConsentDefaultState_OverrideForStore = _settingService.SettingExists(facebookPixelSettings, x => x.ConsentDefaultState, storeScope);
                model.ConsentName_OverrideForStore = _settingService.SettingExists(facebookPixelSettings, x => x.ConsentName, storeScope);
                model.ConsentDescription_OverrideForStore = _settingService.SettingExists(facebookPixelSettings, x => x.ConsentDescription, storeScope);

            }
            return View("~/Plugins/Widgets.FacebookPixel/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var facebookPixelSettings = _settingService.LoadSetting<FacebookPixelSettings>(storeScope);
            facebookPixelSettings.PixelId = model.PixelId;
            facebookPixelSettings.PixelScript = model.PixelScript;
            facebookPixelSettings.AddToCartScript = model.AddToCartScript;
            facebookPixelSettings.DetailsOrderScript = model.DetailsOrderScript;
            facebookPixelSettings.AllowToDisableConsentCookie = model.AllowToDisableConsentCookie;
            facebookPixelSettings.ConsentName = model.ConsentName;
            facebookPixelSettings.ConsentDescription = model.ConsentDescription;
            facebookPixelSettings.ConsentDefaultState = model.ConsentDefaultState;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.PixelId_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(facebookPixelSettings, x => x.PixelId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(facebookPixelSettings, x => x.PixelId, storeScope);

            if (model.PixelScript_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(facebookPixelSettings, x => x.PixelScript, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(facebookPixelSettings, x => x.PixelScript, storeScope);

            if (model.AddToCartScript_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(facebookPixelSettings, x => x.AddToCartScript, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(facebookPixelSettings, x => x.AddToCartScript, storeScope);

            if (model.DetailsOrderScript_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(facebookPixelSettings, x => x.DetailsOrderScript, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(facebookPixelSettings, x => x.DetailsOrderScript, storeScope);

            if (model.AllowToDisableConsentCookie_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(facebookPixelSettings, x => x.AllowToDisableConsentCookie, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(facebookPixelSettings, x => x.AllowToDisableConsentCookie, storeScope);

            if (model.ConsentDefaultState_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(facebookPixelSettings, x => x.ConsentDefaultState, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(facebookPixelSettings, x => x.ConsentDefaultState, storeScope);

            if (model.ConsentName_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(facebookPixelSettings, x => x.ConsentName, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(facebookPixelSettings, x => x.ConsentName, storeScope);

            if (model.ConsentDescription_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(facebookPixelSettings, x => x.ConsentDescription, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(facebookPixelSettings, x => x.ConsentDescription, storeScope);

            //now clear settings cache
            await _settingService.ClearCache();
            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return await Configure();
        }

    }
}