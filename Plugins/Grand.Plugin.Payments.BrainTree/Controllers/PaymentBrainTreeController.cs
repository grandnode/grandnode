using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.Payments.BrainTree.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Plugin.Payments.BrainTree.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public class PaymentBrainTreeController : BasePaymentController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly BrainTreePaymentSettings _brainTreePaymentSettings;
        #endregion

        #region Ctor

        public PaymentBrainTreeController(ISettingService settingService,
            ILocalizationService localizationService, 
            IPermissionService permissionService,
            BrainTreePaymentSettings brainTreePaymentSettings)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _brainTreePaymentSettings = brainTreePaymentSettings;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                Use3DS = _brainTreePaymentSettings.Use3DS,
                UseSandBox = _brainTreePaymentSettings.UseSandBox,
                PublicKey = _brainTreePaymentSettings.PublicKey,
                PrivateKey = _brainTreePaymentSettings.PrivateKey,
                MerchantId = _brainTreePaymentSettings.MerchantId,
                AdditionalFee = _brainTreePaymentSettings.AdditionalFee,
                AdditionalFeePercentage = _brainTreePaymentSettings.AdditionalFeePercentage
            };

            return View("~/Plugins/Payments.BrainTree/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //save settings
            _brainTreePaymentSettings.Use3DS = model.Use3DS;
            _brainTreePaymentSettings.UseSandBox = model.UseSandBox;
            _brainTreePaymentSettings.PublicKey = model.PublicKey;
            _brainTreePaymentSettings.PrivateKey = model.PrivateKey;
            _brainTreePaymentSettings.MerchantId = model.MerchantId;
            _brainTreePaymentSettings.AdditionalFee = model.AdditionalFee;
            _brainTreePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            await _settingService.SaveSetting(_brainTreePaymentSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion
    }
}