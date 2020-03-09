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
    /// Represents filter attribute that saves last customer activity date
    /// </summary>
    public class SaveLastActivityAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public SaveLastActivityAttribute() : base(typeof(SaveLastActivityFilter))
        {
        }

        #region Nested filter

        /// <summary>
        /// Represents a filter that saves last customer activity date
        /// </summary>
        private class SaveLastActivityFilter : IAsyncActionFilter
        {
            #region Fields

            private readonly ICustomerService _customerService;
            private readonly IWorkContext _workContext;
            private readonly ICustomerActionEventService _customerActionEventService;

            #endregion

            #region Ctor

            public SaveLastActivityFilter(ICustomerService customerService,
                IWorkContext workContext,
                ICustomerActionEventService customerActionEventService)
            {
                _customerService = customerService;
                _workContext = workContext;
                _customerActionEventService = customerActionEventService;
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

                //update last activity date
                if (_workContext.CurrentCustomer.LastActivityDateUtc.AddMinutes(1.0) < DateTime.UtcNow)
                {
                    _workContext.CurrentCustomer.LastActivityDateUtc = DateTime.UtcNow;
                    await _customerService.UpdateCustomerLastActivityDate(_workContext.CurrentCustomer);
                }

                await _customerActionEventService.Url(_workContext.CurrentCustomer, context.HttpContext?.Request?.Path.ToString(), context.HttpContext?.Request?.Headers["Referer"]);
            }

           
            #endregion
        }

        #endregion
    }
}