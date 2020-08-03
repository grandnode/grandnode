using Grand.Core;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Grand.Framework.Middleware
{
    public class StoreContextMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next">Next</param>
        public StoreContextMiddleware(RequestDelegate next)
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
        public async Task Invoke(HttpContext context, IStoreContext storeContext)
        {
            if (context == null || context.Request == null)
                return;

            await storeContext.SetCurrentStore();

            //call the next middleware in the request pipeline
            await _next(context);
        }

        #endregion
    }
}
