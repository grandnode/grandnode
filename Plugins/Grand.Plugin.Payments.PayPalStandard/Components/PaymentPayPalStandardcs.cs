using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.Payments.PayPalStandard.Controllers
{
    [ViewComponent(Name = "PaymentPayPalStandard")]
    public class PaymentPayPalStandardViewComponent : ViewComponent
    {
        public PaymentPayPalStandardViewComponent() { }

        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.PayPalStandard/netcoreapp2.0/Views/PaymentPayPalStandard/PaymentInfo.cshtml");
        }
    }
}