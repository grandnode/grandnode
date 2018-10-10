using Grand.Core;
using Grand.Core.Data;
using Grand.Services.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents filter attribute that validates customer password expiration
    /// </summary>
    public class ValidatePasswordAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public ValidatePasswordAttribute() : base(typeof(ValidatePasswordFilter))
        {
        }

        #region Nested filter

        /// <summary>
        /// Represents a filter that validates customer password expiration
        /// </summary>
        private class ValidatePasswordFilter : IActionFilter
        {
            #region Fields

            private readonly IWorkContext _workContext;

            #endregion

            #region Ctor

            public ValidatePasswordFilter(IWorkContext workContext)
            {
                this._workContext = workContext;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null || context.HttpContext == null || context.HttpContext.Request == null)
                    return;

                if (!DataSettingsHelper.DatabaseIsInstalled())
                    return;

                //get action and controller names
                var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = actionDescriptor?.ActionName;
                var controllerName = actionDescriptor?.ControllerName;

                if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                    return;
                
                //don't validate on ChangePassword page
                if (!(controllerName.Equals("Customer", StringComparison.OrdinalIgnoreCase) &&
                    actionName.Equals("ChangePassword", StringComparison.OrdinalIgnoreCase)))
                {
                    //check password expiration
                    if (_workContext.CurrentCustomer.PasswordIsExpired())
                    {
                        //redirect to ChangePassword page if expires
                        var changePasswordUrl = new UrlHelper(context).RouteUrl("CustomerChangePassword");
                        context.Result = new RedirectResult(changePasswordUrl);
                    }
                }
            }

            /// <summary>
            /// Called after the action executes, before the action result
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuted(ActionExecutedContext context)
            {
                //do nothing
            }

            #endregion
        }

        #endregion
    }
}