using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.Payments.CashOnDelivery.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Grand.Plugin.Payments.CashOnDelivery.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public class PaymentCashOnDeliveryController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;


        public PaymentCashOnDeliveryController(IWorkContext workContext,
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
        
        public IActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var cashOnDeliveryPaymentSettings = _settingService.LoadSetting<CashOnDeliveryPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.DescriptionText = cashOnDeliveryPaymentSettings.DescriptionText;
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.DescriptionText = cashOnDeliveryPaymentSettings.GetLocalizedSetting(x => x.DescriptionText, languageId, "", false, false);
            });
            model.AdditionalFee = cashOnDeliveryPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = cashOnDeliveryPaymentSettings.AdditionalFeePercentage;
            model.ShippableProductRequired = cashOnDeliveryPaymentSettings.ShippableProductRequired;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.DescriptionText_OverrideForStore = _settingService.SettingExists(cashOnDeliveryPaymentSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(cashOnDeliveryPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(cashOnDeliveryPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.ShippableProductRequired_OverrideForStore = _settingService.SettingExists(cashOnDeliveryPaymentSettings, x => x.ShippableProductRequired, storeScope);
            }

            return View("~/Plugins/Payments.CashOnDelivery/Views/PaymentCashOnDelivery/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var cashOnDeliveryPaymentSettings = _settingService.LoadSetting<CashOnDeliveryPaymentSettings>(storeScope);

            //save settings
            cashOnDeliveryPaymentSettings.DescriptionText = model.DescriptionText;
            cashOnDeliveryPaymentSettings.AdditionalFee = model.AdditionalFee;
            cashOnDeliveryPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            cashOnDeliveryPaymentSettings.ShippableProductRequired = model.ShippableProductRequired;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.DescriptionText_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(cashOnDeliveryPaymentSettings, x => x.DescriptionText, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(cashOnDeliveryPaymentSettings, x => x.DescriptionText, storeScope);

            if (model.AdditionalFee_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(cashOnDeliveryPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(cashOnDeliveryPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(cashOnDeliveryPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(cashOnDeliveryPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.ShippableProductRequired_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(cashOnDeliveryPaymentSettings, x => x.ShippableProductRequired, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(cashOnDeliveryPaymentSettings, x => x.ShippableProductRequired, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
       
    }
}
