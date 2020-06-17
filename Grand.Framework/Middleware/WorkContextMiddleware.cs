using Grand.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Grand.Framework.Middleware
{
    public class WorkContextMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next">Next</param>
        public WorkContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke middleware actions
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task Invoke(HttpContext context, IWorkContext workContext)
        {
            if (context == null || context.Request == null)
            {
                await _next(context);
                return;
            }

            //set current customer
            var customer = await workContext.SetCurrentCustomer();
            var vendor = await workContext.SetCurrentVendor(customer);
            var language = await workContext.SetWorkingLanguage(customer);
            var currency = await workContext.SetWorkingCurrency(customer);
            var taxtype = await workContext.SetTaxDisplayType(customer);

            //set culture in admin area
            if (context.Request.Path.Value.StartsWith("/admin", StringComparison.InvariantCultureIgnoreCase))
            {
                var culture = new CultureInfo("en-US");
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            else
            {
                //set culture for customer
                if (!string.IsNullOrEmpty(language?.LanguageCulture))
                {
                    var culture = new CultureInfo(language.LanguageCulture);
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                }
            }
            //call the next middleware in the request pipeline
            await _next(context);
        }

        #endregion
    }
}
