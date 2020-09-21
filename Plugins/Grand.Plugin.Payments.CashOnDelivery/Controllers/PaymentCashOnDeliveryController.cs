using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Plugin.Payments.CashOnDelivery.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.Payments.CashOnDelivery.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    [PermissionAuthorize(PermissionSystemName.PaymentMethods)]
    public class PaymentCashOnDeliveryController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;


        public PaymentCashOnDeliveryController(IWorkContext workContext,
            IStoreService storeService, 
            ISettingService settingService,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _localizationService = localizationService;
            _languageService = languageService;
        }
        
        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var cashOnDeliveryPaymentSettings = _settingService.LoadSetting<CashOnDeliveryPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.DescriptionText = cashOnDeliveryPaymentSettings.DescriptionText;
            //locales
            await AddLocales(_languageService, model.Locales, async (locale, languageId) => 
            {
                locale.DescriptionText = await cashOnDeliveryPaymentSettings.GetLocalizedSetting(_settingService, x => x.DescriptionText, languageId, "", false, false);
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
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
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
                await _settingService.SaveSetting(cashOnDeliveryPaymentSettings, x => x.DescriptionText, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(cashOnDeliveryPaymentSettings, x => x.DescriptionText, storeScope);

            if (model.AdditionalFee_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(cashOnDeliveryPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(cashOnDeliveryPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(cashOnDeliveryPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(cashOnDeliveryPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.ShippableProductRequired_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(cashOnDeliveryPaymentSettings, x => x.ShippableProductRequired, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(cashOnDeliveryPaymentSettings, x => x.ShippableProductRequired, storeScope);

            //now clear settings cache
            await _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();
        }
       
    }
}
