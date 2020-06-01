using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Framework.Infrastructure.Extensions;
using Grand.Framework.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Framework.StartupConfigure
{
    /// <summary>
    /// Represents object for the configuring authentication middleware on application startup
    /// </summary>
    public class AuthenticationStartup : IGrandStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var config = new GrandConfig();
            configuration.GetSection("Grand").Bind(config);

            //add data protection
            services.AddGrandDataProtection(config);
            //add authentication
            services.AddGrandAuthentication(configuration);
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {
            //check whether database is installed
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            //configure authentication
            application.UseGrandAuthentication();

            //set storecontext
            application.UseMiddleware<StoreContextMiddleware>();

            //set workcontext
            application.UseMiddleware<WorkContextMiddleware>();

        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order
        {
            //authentication should be loaded before MVC
            get { return 500; }
        }
    }
}
