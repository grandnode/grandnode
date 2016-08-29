﻿using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Plugin.Widgets.NivoSlider.Infrastructure.Cache;
using Grand.Plugin.Widgets.NivoSlider.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Stores;
using Grand.Web.Framework.Controllers;
using System;

namespace Grand.Plugin.Widgets.NivoSlider.Controllers
{
    public class WidgetsNivoSliderController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;

        public WidgetsNivoSliderController(IWorkContext workContext,
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

        protected string GetPictureUrl(string pictureId)
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.PICTURE_URL_MODEL_KEY, pictureId);
            return _cacheManager.Get(cacheKey, () =>
            {
                var url = _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false);
                //little hack here. nulls aren't cacheable so set it to ""
                if (url == null)
                    url = "";

                return url;
            });
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var nivoSliderSettings = _settingService.LoadSetting<NivoSliderSettings>(storeScope);
            var model = new ConfigurationModel();
            model.Picture1Id = nivoSliderSettings.Picture1Id;
            model.Text1 = nivoSliderSettings.Text1;
            model.Link1 = nivoSliderSettings.Link1;
            model.Picture2Id = nivoSliderSettings.Picture2Id;
            model.Text2 = nivoSliderSettings.Text2;
            model.Link2 = nivoSliderSettings.Link2;
            model.Picture3Id = nivoSliderSettings.Picture3Id;
            model.Text3 = nivoSliderSettings.Text3;
            model.Link3 = nivoSliderSettings.Link3;
            model.Picture4Id = nivoSliderSettings.Picture4Id;
            model.Text4 = nivoSliderSettings.Text4;
            model.Link4 = nivoSliderSettings.Link4;
            model.Picture5Id = nivoSliderSettings.Picture5Id;
            model.Text5 = nivoSliderSettings.Text5;
            model.Link5 = nivoSliderSettings.Link5;
            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.Picture1Id_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Picture1Id, storeScope);
                model.Text1_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Text1, storeScope);
                model.Link1_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Link1, storeScope);
                model.Picture2Id_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Picture2Id, storeScope);
                model.Text2_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Text2, storeScope);
                model.Link2_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Link2, storeScope);
                model.Picture3Id_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Picture3Id, storeScope);
                model.Text3_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Text3, storeScope);
                model.Link3_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Link3, storeScope);
                model.Picture4Id_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Picture4Id, storeScope);
                model.Text4_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Text4, storeScope);
                model.Link4_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Link4, storeScope);
                model.Picture5Id_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Picture5Id, storeScope);
                model.Text5_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Text5, storeScope);
                model.Link5_OverrideForStore = _settingService.SettingExists(nivoSliderSettings, x => x.Link5, storeScope);
            }

            return View("~/Plugins/Widgets.NivoSlider/Views/WidgetsNivoSlider/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var nivoSliderSettings = _settingService.LoadSetting<NivoSliderSettings>(storeScope);
            nivoSliderSettings.Picture1Id = model.Picture1Id;
            nivoSliderSettings.Text1 = model.Text1;
            nivoSliderSettings.Link1 = model.Link1;
            nivoSliderSettings.Picture2Id = model.Picture2Id;
            nivoSliderSettings.Text2 = model.Text2;
            nivoSliderSettings.Link2 = model.Link2;
            nivoSliderSettings.Picture3Id = model.Picture3Id;
            nivoSliderSettings.Text3 = model.Text3;
            nivoSliderSettings.Link3 = model.Link3;
            nivoSliderSettings.Picture4Id = model.Picture4Id;
            nivoSliderSettings.Text4 = model.Text4;
            nivoSliderSettings.Link4 = model.Link4;
            nivoSliderSettings.Picture5Id = model.Picture5Id;
            nivoSliderSettings.Text5 = model.Text5;
            nivoSliderSettings.Link5 = model.Link5;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.Picture1Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Picture1Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Picture1Id, storeScope);
            
            if (model.Text1_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Text1, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Text1, storeScope);
            
            if (model.Link1_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Link1, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Link1, storeScope);
            
            if (model.Picture2Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Picture2Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Picture2Id, storeScope);
            
            if (model.Text2_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Text2, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Text2, storeScope);
            
            if (model.Link2_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Link2, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Link2, storeScope);
            
            if (model.Picture3Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Picture3Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Picture3Id, storeScope);
            
            if (model.Text3_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Text3, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Text3, storeScope);
            
            if (model.Link3_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Link3, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Link3, storeScope);
            
            if (model.Picture4Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Picture4Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Picture4Id, storeScope);
            
            if (model.Text4_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Text4, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Text4, storeScope);

            if (model.Link4_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Link4, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Link4, storeScope);

            if (model.Picture5Id_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Picture5Id, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Picture5Id, storeScope);

            if (model.Text5_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Text5, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Text5, storeScope);

            if (model.Link5_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(nivoSliderSettings, x => x.Link5, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(nivoSliderSettings, x => x.Link5, storeScope);
            
            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            var nivoSliderSettings = _settingService.LoadSetting<NivoSliderSettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel();
            model.Picture1Url = GetPictureUrl(nivoSliderSettings.Picture1Id);
            model.Text1 = nivoSliderSettings.Text1;
            model.Link1 = nivoSliderSettings.Link1;

            model.Picture2Url = GetPictureUrl(nivoSliderSettings.Picture2Id);
            model.Text2 = nivoSliderSettings.Text2;
            model.Link2 = nivoSliderSettings.Link2;

            model.Picture3Url = GetPictureUrl(nivoSliderSettings.Picture3Id);
            model.Text3 = nivoSliderSettings.Text3;
            model.Link3 = nivoSliderSettings.Link3;

            model.Picture4Url = GetPictureUrl(nivoSliderSettings.Picture4Id);
            model.Text4 = nivoSliderSettings.Text4;
            model.Link4 = nivoSliderSettings.Link4;

            model.Picture5Url = GetPictureUrl(nivoSliderSettings.Picture5Id);
            model.Text5 = nivoSliderSettings.Text5;
            model.Link5 = nivoSliderSettings.Link5;

            if (string.IsNullOrEmpty(model.Picture1Url) && string.IsNullOrEmpty(model.Picture2Url) &&
                string.IsNullOrEmpty(model.Picture3Url) && string.IsNullOrEmpty(model.Picture4Url) &&
                string.IsNullOrEmpty(model.Picture5Url))
                //no pictures uploaded
                return Content("");


            return View("~/Plugins/Widgets.NivoSlider/Views/WidgetsNivoSlider/PublicInfo.cshtml", model);
        }
    }
}