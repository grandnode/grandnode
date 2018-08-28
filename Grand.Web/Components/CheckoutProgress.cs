using Microsoft.AspNetCore.Mvc;
using Grand.Web.Models.Checkout;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class CheckoutProgressViewComponent : BaseViewComponent
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