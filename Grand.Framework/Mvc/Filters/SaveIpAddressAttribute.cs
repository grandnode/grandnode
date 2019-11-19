using Grand.Core;
using Grand.Core.Data;
using Grand.Services.Customers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents filter attribute that saves last IP address of customer
    /// </summary>
    public class SaveIpAddressAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public SaveIpAddressAttribute() : base(typeof(SaveIpAddressFilter))
        {
        }

        #region Nested filter

        /// <summary>
        /// Represents a filter that saves last IP address of customer
        /// </summary>
        private class SaveIpAddressFilter : IAsyncActionFilter
        {
            #region Fields

            private readonly ICustomerService _customerService;
            private readonly IWebHelper _webHelper;
            private readonly IWorkContext _workContext;

            #endregion

            #region Ctor

            public SaveIpAddressFilter(ICustomerService customerService,
                IWebHelper webHelper,
                IWorkContext workContext)
            {
                _customerService = customerService;
                _webHelper = webHelper;
                _workContext = workContext;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                await next();

                if (context == null || context.HttpContext == null || context.HttpContext.Request == null)
                    return;

                if (!DataSettingsHelper.DatabaseIsInstalled())
                    return;

                //only in GET requests
                if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
                    return;

                //get current IP address
                var currentIpAddress = _webHelper.GetCurrentIpAddress();
                if (string.IsNullOrEmpty(currentIpAddress))
                    return;
                
                //update customer's IP address
                if (!currentIpAddress.Equals(_workContext.CurrentCustomer.LastIpAddress, StringComparison.OrdinalIgnoreCase))
                {
                    _workContext.CurrentCustomer.LastIpAddress = currentIpAddress;
                    await _customerService.UpdateCustomerLastIpAddress(_workContext.CurrentCustomer);
                }
            }
            #endregion
        }

        #endregion
    }
}