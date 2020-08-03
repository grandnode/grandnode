using Grand.Core;
using Grand.Plugin.Payments.CheckMoneyOrder.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Plugin.Payments.CheckMoneyOrder.Components
{
    public class PaymentCheckMoneyOrderViewComponent : ViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        public PaymentCheckMoneyOrderViewComponent(IWorkContext workContext,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var checkMoneyOrderPaymentSettings = _settingService.LoadSetting<CheckMoneyOrderPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel
            {
                DescriptionText = await checkMoneyOrderPaymentSettings.GetLocalizedSetting(_settingService, x => x.DescriptionText, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id)
            };

            return View("~/Plugins/Payments.CheckMoneyOrder/Views/PaymentCheckMoneyOrder/PaymentInfo.cshtml", await Task.FromResult(model));
        }
    }
}