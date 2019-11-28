using Grand.Core;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Grand.Framework.Middleware
{
    public class WorkContextMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;
        //private readonly IStoreContext _storeContext;
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
        public async Task InvokeAsync(HttpContext context, IWorkContext workContext)
        {
            //set current customer
            var customer = await workContext.SetCurrentCustomer();
            var vendor = await workContext.SetCurrentVendor(customer);
            var language = await workContext.SetWorkingLanguage(customer);
            var currency = await workContext.SetWorkingCurrency(customer);
            var taxtype = await workContext.SetTaxDisplayType(customer);

            //call the next middleware in the request pipeline
            await _next(context);
        }

        #endregion
    }
}
