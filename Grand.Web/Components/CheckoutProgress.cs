using Microsoft.AspNetCore.Mvc;
using Grand.Web.Models.Checkout;

namespace Grand.Web.ViewComponents
{
    public class CheckoutProgressViewComponent : ViewComponent
    {
        public CheckoutProgressViewComponent()
        {
        }

        public IViewComponentResult Invoke(CheckoutProgressStep step)
        {
            var model = new CheckoutProgressModel { CheckoutProgressStep = step };
            return View(model);

        }
    }
}