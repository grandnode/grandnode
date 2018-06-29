using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.Payments.CheckMoneyOrder.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Grand.Plugin.Payments.CheckMoneyOrder.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public class PaymentCheckMoneyOrderController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public PaymentCheckMoneyOrderController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._languageService = languageService;
        }
        
        public IActionResult  Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var checkMoneyOrderPaymentSettings = _settingService.LoadSetting<CheckMoneyOrderPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.DescriptionText = checkMoneyOrderPaymentSettings.DescriptionText;
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.DescriptionText = checkMoneyOrderPaymentSettings.GetLocalizedSetting(x => x.DescriptionText, languageId, "", false, false);
            });
            model.AdditionalFee = checkMoneyOrderPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = checkMoneyOrderPaymentSettings.AdditionalFeePercentage;
            model.ShippableProductRequired = checkMoneyOrderPaymentSettings.ShippableProductRequired;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.DescriptionText_OverrideForStore = _settingService.SettingExists(checkMoneyOrderPaymentSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(checkMoneyOrderPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(checkMoneyOrderPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.ShippableProductRequired_OverrideForStore = _settingService.SettingExists(checkMoneyOrderPaymentSettings, x => x.ShippableProductRequired, storeScope);
            }

            return View("~/Plugins/Payments.CheckMoneyOrder/Views/PaymentCheckMoneyOrder/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult  Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var checkMoneyOrderPaymentSettings = _settingService.LoadSetting<CheckMoneyOrderPaymentSettings>(storeScope);

            //save settings
            checkMoneyOrderPaymentSettings.DescriptionText = model.DescriptionText;
            checkMoneyOrderPaymentSettings.AdditionalFee = model.AdditionalFee;
            checkMoneyOrderPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            checkMoneyOrderPaymentSettings.ShippableProductRequired = model.ShippableProductRequired;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.DescriptionText_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(checkMoneyOrderPaymentSettings, x => x.DescriptionText, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(checkMoneyOrderPaymentSettings, x => x.DescriptionText, storeScope);

            if (model.AdditionalFee_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(checkMoneyOrderPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(checkMoneyOrderPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(checkMoneyOrderPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(checkMoneyOrderPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.ShippableProductRequired_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(checkMoneyOrderPaymentSettings, x => x.ShippableProductRequired, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(checkMoneyOrderPaymentSettings, x => x.ShippableProductRequired, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}
