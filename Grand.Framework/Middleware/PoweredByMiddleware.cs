//Contribution: Orchard project (https://github.com/OrchardCMS/OrchardCore)
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Grand.Framework.Middleware
{
    /// <summary>
    /// Adds the X-Powered-By header with values grandnode.
    /// </summary>
    public class PoweredByMiddleware
    {
        private readonly RequestDelegate _next;

        public PoweredByMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IPoweredByMiddlewareOptions options)
        {
            if (options.Enabled)
            {
                context.Response.Headers[options.HeaderName] = options.HeaderValue;
            }

            await _next.Invoke(context);
        }
    }

    public interface IPoweredByMiddlewareOptions
    {
        bool Enabled { get; set; }
        string HeaderName { get; }
        string HeaderValue { get; set; }
    }

    public class PoweredByMiddlewareOptions: IPoweredByMiddlewareOptions
    {
        const string PoweredByHeaderName = "X-Powered-By";
        const string PoweredByHeaderValue = "GrandNode";

        public string HeaderName => PoweredByHeaderName;
        public string HeaderValue { get; set; } = PoweredByHeaderValue;

        public bool Enabled { get; set; } = true;
    }
}