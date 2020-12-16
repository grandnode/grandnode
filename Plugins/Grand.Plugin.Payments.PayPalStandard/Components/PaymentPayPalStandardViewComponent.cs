using Grand.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.Payments.PayPalStandard.Controllers
{
    [ViewComponent(Name = "PaymentPayPalStandard")]
    public class PaymentPayPalStandardViewComponent : BaseViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.PayPalStandard/Views/PaymentPayPalStandard/PaymentInfo.cshtml");
        }
    }
}