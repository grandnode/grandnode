using Grand.Framework.Components;
using Grand.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class CheckoutProgressViewComponent : BaseViewComponent
    {
        public IViewComponentResult Invoke(CheckoutProgressStep step)
        {
            var model = new CheckoutProgressModel { CheckoutProgressStep = step };
            return View(model);

        }
    }
}