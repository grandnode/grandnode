using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;

namespace Grand.Web.Controllers
{
    [WwwRequirement]
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
