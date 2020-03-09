﻿using Grand.Core;
using Grand.Core.Domain.Security;
using Grand.Services.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents a filter attribute enabling honeypot validation
    /// </summary>
    public class ValidateHoneypotAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public ValidateHoneypotAttribute() : base(typeof(ValidateHoneypotFilter))
        {
        }

        #region Nested filter

        /// <summary>
        /// Represents a filter enabling honeypot validation
        /// </summary>
        private class ValidateHoneypotFilter : IAuthorizationFilter
        {
            #region Fields

            private readonly ILogger _logger;
            private readonly IWebHelper _webHelper;
            private readonly SecuritySettings _securitySettings;

            #endregion

            #region Ctor

            public ValidateHoneypotFilter(ILogger logger,
                IWebHelper webHelper,
                SecuritySettings securitySettings)
            {
                _logger = logger;
                _webHelper = webHelper;
                _securitySettings = securitySettings;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called early in the filter pipeline to confirm request is authorized
            /// </summary>
            /// <param name="filterContext">Authorization filter context</param>
            public void OnAuthorization(AuthorizationFilterContext filterContext)
            {
                if (filterContext == null || filterContext.HttpContext.Request == null)
                    throw new ArgumentNullException("filterContext");

                //whether honeypot is enabled
                if (!_securitySettings.HoneypotEnabled)
                    return;

                //try get honeypot input value 
                var inputValue = filterContext.HttpContext.Request.Form[_securitySettings.HoneypotInputName];

                //if exists, bot is caught
                if (!StringValues.IsNullOrEmpty(inputValue))
                {
                    //warning admin about it
                    _logger.Warning("A bot detected. Honeypot.");

                    //and redirect to the original page
                    filterContext.Result = new RedirectResult(_webHelper.GetThisPageUrl(true));
                }
            }

            #endregion
        }

        #endregion
    }
}