﻿using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.Widgets.GoogleAnalytics.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.Widgets.GoogleAnalytics.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    public class WidgetsGoogleAnalyticsController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public WidgetsGoogleAnalyticsController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILogger logger,
            ILocalizationService localizationService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _localizationService = localizationService;
        }

        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsEcommerceSettings>(storeScope);
            var model = new ConfigurationModel();
            model.GoogleId = googleAnalyticsSettings.GoogleId;
            model.TrackingScript = googleAnalyticsSettings.TrackingScript;
            model.EcommerceScript = googleAnalyticsSettings.EcommerceScript;
            model.EcommerceDetailScript = googleAnalyticsSettings.EcommerceDetailScript;
            model.IncludingTax = googleAnalyticsSettings.IncludingTax;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.GoogleId_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.GoogleId, storeScope);
                model.TrackingScript_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.TrackingScript, storeScope);
                model.EcommerceScript_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.EcommerceScript, storeScope);
                model.EcommerceDetailScript_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.EcommerceDetailScript, storeScope);
                model.IncludingTax_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.IncludingTax, storeScope);
            }

            return View("~/Plugins/Widgets.GoogleAnalytics/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsEcommerceSettings>(storeScope);
            googleAnalyticsSettings.GoogleId = model.GoogleId;
            googleAnalyticsSettings.TrackingScript = model.TrackingScript;
            googleAnalyticsSettings.EcommerceScript = model.EcommerceScript;
            googleAnalyticsSettings.EcommerceDetailScript = model.EcommerceDetailScript;
            googleAnalyticsSettings.IncludingTax = model.IncludingTax;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.GoogleId_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(googleAnalyticsSettings, x => x.GoogleId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(googleAnalyticsSettings, x => x.GoogleId, storeScope);

            if (model.TrackingScript_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(googleAnalyticsSettings, x => x.TrackingScript, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(googleAnalyticsSettings, x => x.TrackingScript, storeScope);

            if (model.EcommerceScript_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(googleAnalyticsSettings, x => x.EcommerceScript, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(googleAnalyticsSettings, x => x.EcommerceScript, storeScope);

            if (model.EcommerceDetailScript_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(googleAnalyticsSettings, x => x.EcommerceDetailScript, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(googleAnalyticsSettings, x => x.EcommerceDetailScript, storeScope);

            if (model.IncludingTax_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(googleAnalyticsSettings, x => x.IncludingTax, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(googleAnalyticsSettings, x => x.IncludingTax, storeScope);

            //now clear settings cache
            await _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();
        }
    }
}