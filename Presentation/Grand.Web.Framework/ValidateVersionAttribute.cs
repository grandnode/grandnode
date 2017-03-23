using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Services.Customers;
using System;
using System.Web.Mvc;

namespace Grand.Web.Framework
{
    /// <summary>
    /// Represents filter attribute to validate customer password expiration
    /// </summary>
    public class ValidateVersionAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Called by the ASP.NET MVC framework before the action method executes
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var controllerName = filterContext.Controller.ToString();
            if (string.IsNullOrEmpty(controllerName) || controllerName.Equals("Grand.Web.Controllers.UpgradeController", StringComparison.InvariantCultureIgnoreCase))
                return;

            var upgradeUrl = new UrlHelper(filterContext.RequestContext).RouteUrl("Upgrade");
            filterContext.Result = new RedirectResult(upgradeUrl);
        }
    }
}
