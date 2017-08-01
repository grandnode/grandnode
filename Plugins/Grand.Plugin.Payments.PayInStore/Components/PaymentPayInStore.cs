using Grand.Plugin.Payments.PayInStore.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Plugin.Payments.PayInStore.Components
{
    [ViewComponent(Name = "PaymentPayInStore")]
    public class PaymentPayInStoreViewComponent : ViewComponent
    {
        private readonly PayInStorePaymentSettings _payInStorePaymentSettings;
        public PaymentPayInStoreViewComponent(PayInStorePaymentSettings payInStorePaymentSettings)
        {
            this._payInStorePaymentSettings = payInStorePaymentSettings;
        }
        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel()
            {
                DescriptionText = _payInStorePaymentSettings.DescriptionText
            };
            return View("/Plugins/Payments.PayInStore/netcoreapp1.1/Views/PaymentInfo.cshtml", model);
        }
    }
}
