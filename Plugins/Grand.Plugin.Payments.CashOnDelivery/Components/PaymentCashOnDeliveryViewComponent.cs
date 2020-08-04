using Grand.Core;
using Grand.Plugin.Payments.CashOnDelivery.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            _workContext = workContext;
            _settingService = settingService;
            _storeContext = storeContext;

        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cashOnDeliveryPaymentSettings = _settingService.LoadSetting<CashOnDeliveryPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel
            {
                DescriptionText = await cashOnDeliveryPaymentSettings.GetLocalizedSetting(_settingService, x => x.DescriptionText, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id)
            };
            return View("~/Plugins/Payments.CashOnDelivery/Views/PaymentCashOnDelivery/PaymentInfo.cshtml", await Task.FromResult(model));
        }
    }
}