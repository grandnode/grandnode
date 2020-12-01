using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Controllers
{
    [CheckAccessPublicStore]
    [CheckAccessClosedStore]
    [CheckLanguageSeoCode]
    [CheckAffiliate]
    public abstract partial class BasePublicController : BaseController
    {
        protected virtual IActionResult InvokeHttp404()
        {
            Response.StatusCode = 404;
            return new EmptyResult();
        }

        protected bool IsJsonResponseView()
        {
            var viewJson = Request?.Headers["X-Response-View"];
            if (viewJson?.Equals("Json") ?? false)
            {
                return true;
            }
            return false;
        }

        public new IActionResult View(object model)
        {
            if (CommonHelper.AllowToJsonResponse && IsJsonResponseView())
                return Json(model);

            return base.View(model);
        }

        public new IActionResult View(string viewName, object model)
        {
            if (CommonHelper.AllowToJsonResponse && IsJsonResponseView())
                return Json(model);

            return base.View(viewName, model);
        }
    }
}
