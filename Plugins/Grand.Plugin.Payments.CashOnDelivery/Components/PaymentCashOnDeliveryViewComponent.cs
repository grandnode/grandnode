using Grand.Core;
using Grand.Plugin.Payments.CashOnDelivery.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.Payments.CashOnDelivery.Components
{
    public class PaymentCashOnDeliveryViewComponent : ViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public PaymentCashOnDeliveryViewComponent(IWorkContext workContext,   
            ISettingService settingService,
            IStoreContext storeContext)
        {
            this._workContext = workContext;
            this._settingService = settingService;
            this._storeContext = storeContext;

        }

        public IViewComponentResult Invoke()
        {
            var cashOnDeliveryPaymentSettings = _settingService.LoadSetting<CashOnDeliveryPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel
            {
                DescriptionText = cashOnDeliveryPaymentSettings.GetLocalizedSetting(x=>x.DescriptionText, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id)
            };

            return View("~/Plugins/Payments.CashOnDelivery/Views/PaymentCashOnDelivery/PaymentInfo.cshtml", model);
        }
    }
}