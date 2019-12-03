using Grand.Core;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Grand.Framework.Middleware
{
    public class StoreContextMiddleware
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
        public async Task InvokeAsync(HttpContext context, IStoreContext storeContext)
        {
            await storeContext.SetCurrentStore();

            //call the next middleware in the request pipeline
            await _next(context);
        }

        #endregion
    }
}
