﻿using Grand.Core;
using Grand.Core.Domain.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents filter attribute that validates IP address
    /// </summary>
    public class ValidateIpAddressAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public ValidateIpAddressAttribute() : base(typeof(ValidateIpAddressFilter))
        {
        }

        #region Nested filter

        /// <summary>
        /// Represents a filter that validates IP address
        /// </summary>
        private class ValidateIpAddressFilter : IActionFilter
        {
            #region Fields
            
            private readonly IWebHelper _webHelper;
            private readonly SecuritySettings _securitySettings;

            #endregion

            #region Ctor

            public ValidateIpAddressFilter(IWebHelper webHelper,
                SecuritySettings securitySettings)
            {
                _webHelper = webHelper;
                _securitySettings = securitySettings;
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

                //get action and controller names
                var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = actionDescriptor?.ActionName;
                var controllerName = actionDescriptor?.ControllerName;

                if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                    return;

                //get allowed IP addresses
                var ipAddresses = _securitySettings.AdminAreaAllowedIpAddresses;

                //there are no restrictions
                if (ipAddresses == null || !ipAddresses.Any())
                    return;

                //whether current IP is allowed
                var currentIp = _webHelper.GetCurrentIpAddress();
                if (ipAddresses.Any(ip => ip.Equals(currentIp, StringComparison.OrdinalIgnoreCase)))
                    return;

                //ensure that it's not 'Access denied' page
                if (!(controllerName.Equals("Security", StringComparison.OrdinalIgnoreCase) &&
                    actionName.Equals("AccessDenied", StringComparison.OrdinalIgnoreCase)))
                {
                    //redirect to 'Access denied' page
                    context.Result = new RedirectToActionResult("AccessDenied", "Security", context.RouteData.Values);
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