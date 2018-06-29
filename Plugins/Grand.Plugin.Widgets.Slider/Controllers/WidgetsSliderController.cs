using Grand.Core;
using Grand.Core.Caching;
using Grand.Plugin.Widgets.Slider.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Stores;
using Grand.Framework.Controllers;
using System;
using Grand.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.Widgets.Slider.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public class WidgetsSliderController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;

        public WidgetsSliderController(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService, 
            IPictureService pictureService,
            ISettingService settingService,
            ICacheManager cacheManager,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._cacheManager = cacheManager;
            this._localizationService = localizationService;
        }

        public IActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var sliderSettings = _settingService.LoadSetting<SliderSettings>(storeScope);
            var model = new ConfigurationModel();
            model.Picture1Id = sliderSettings.Picture1Id;
            model.Text1 = sliderSettings.Text1;
            model.Link1 = sliderSettings.Link1;
            model.Picture2Id = sliderSettings.Picture2Id;
            model.Text2 = sliderSettings.Text2;
            model.Link2 = sliderSettings.Link2;
            model.Picture3Id = sliderSettings.Picture3Id;
            model.Text3 = sliderSettings.Text3;
            model.Link3 = sliderSettings.Link3;
            model.Picture4Id = sliderSettings.Picture4Id;
            model.Text4 = sliderSettings.Text4;
            model.Link4 = sliderSettings.Link4;
            model.Picture5Id = sliderSettings.Picture5Id;
            model.Text5 = sliderSettings.Text5;
            model.Link5 = sliderSettings.Link5;
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.Picture1Id_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Picture1Id, storeScope);
                model.Text1_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Text1, storeScope);
                model.Link1_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Link1, storeScope);
                model.Picture2Id_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Picture2Id, storeScope);
                model.Text2_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Text2, storeScope);
                model.Link2_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Link2, storeScope);
                model.Picture3Id_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Picture3Id, storeScope);
                model.Text3_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Text3, storeScope);
                model.Link3_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Link3, storeScope);
                model.Picture4Id_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Picture4Id, storeScope);
                model.Text4_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Text4, storeScope);
                model.Link4_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Link4, storeScope);
                model.Picture5Id_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Picture5Id, storeScope);
                model.Text5_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Text5, storeScope);
                model.Link5_OverrideForStore = _settingService.SettingExists(sliderSettings, x => x.Link5, storeScope);
            }

            return View("~/Plugins/Widgets.Slider/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var sliderSettings = _settingService.LoadSetting<SliderSettings>(storeScope);
            sliderSettings.Picture1Id = model.Picture1Id;
            sliderSettings.Text1 = model.Text1;
            sliderSettings.Link1 = model.Link1;
            sliderSettings.Picture2Id = model.Picture2Id;
            sliderSettings.Text2 = model.Text2;
            sliderSettings.Link2 = model.Link2;
            sliderSettings.Picture3Id = model.Picture3Id;
            sliderSettings.Text3 = model.Text3;
            sliderSettings.Link3 = model.Link3;
            sliderSettings.Picture4Id = model.Picture4Id;
            sliderSettings.Text4 = model.Text4;
            sliderSettings.Link4 = model.Link4;
            sliderSettings.Picture5Id = model.Picture5Id;
            sliderSettings.Text5 = model.Text5;
            sliderSettings.Link5 = model.Link5;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.Picture1Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Picture1Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Picture1Id, storeScope);
            
            if (model.Text1_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Text1, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Text1, storeScope);
            
            if (model.Link1_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Link1, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Link1, storeScope);
            
            if (model.Picture2Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Picture2Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Picture2Id, storeScope);
            
            if (model.Text2_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Text2, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Text2, storeScope);
            
            if (model.Link2_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Link2, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Link2, storeScope);
            
            if (model.Picture3Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Picture3Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Picture3Id, storeScope);
            
            if (model.Text3_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Text3, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Text3, storeScope);
            
            if (model.Link3_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Link3, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Link3, storeScope);
            
            if (model.Picture4Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Picture4Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Picture4Id, storeScope);
            
            if (model.Text4_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Text4, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Text4, storeScope);

            if (model.Link4_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Link4, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Link4, storeScope);

            if (model.Picture5Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Picture5Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Picture5Id, storeScope);

            if (model.Text5_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Text5, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Text5, storeScope);

            if (model.Link5_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(sliderSettings, x => x.Link5, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(sliderSettings, x => x.Link5, storeScope);
            
            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

    }
}
