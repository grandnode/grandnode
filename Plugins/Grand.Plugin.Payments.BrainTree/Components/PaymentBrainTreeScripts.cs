using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.Payments.BrainTree.Components
{
    [ViewComponent(Name = "PaymentBrainTreeScripts")]
    public class PaymentBrainTreeScripts : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.BrainTree/Views/PaymentPaymentBrainTreeScripts.cshtml");
        }
    }
}
