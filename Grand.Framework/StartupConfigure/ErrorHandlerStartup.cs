using Grand.Core.Infrastructure;
using Grand.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Framework.StartupConfigure
{
    /// <summary>
    /// Represents object for the configuring exceptions and errors handling on application startup
    /// </summary>
    public class ErrorHandlerStartup : IGrandStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        /// <param name="webHostEnvironment">WebHostEnvironment</param>
        public void Configure(IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {
            //exception handling
            application.UseGrandExceptionHandler();

            //handle 400 errors (bad request)
            application.UseBadRequestResult();

            //handle 404 errors
            application.UsePageNotFound();
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order
        {
            //error handlers should be loaded first
            get { return -10; }
        }
    }
}
