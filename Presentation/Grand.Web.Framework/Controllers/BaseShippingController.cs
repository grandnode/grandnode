using System.Collections.Generic;
using System.Web.Mvc;

namespace Grand.Web.Framework.Controllers
{
    public abstract class BaseShippingController : BasePluginController
    {
        public abstract IList<string> ValidateShippingForm(FormCollection form);
        public abstract JsonResult GetFormPartialView(string shippingOption);
    }
}
