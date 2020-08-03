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
    }
}
