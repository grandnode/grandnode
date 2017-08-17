using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.Payments.PayPalStandard.Controllers
{
    [ViewComponent(Name = "PaymentPayPalStandard")]
    public class PaymentPayPalStandardViewComponent : ViewComponent
    {
        public PaymentPayPalStandardViewComponent() { }

        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.PayPalStandard/netcoreapp1.1/Views/PaymentPayPalStandard/PaymentInfo.cshtml");
        }
    }
}