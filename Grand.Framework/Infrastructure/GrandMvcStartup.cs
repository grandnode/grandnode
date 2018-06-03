using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Grand.Core.Infrastructure;
using Grand.Framework.Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System;
using Grand.Core.Domain;
using Grand.Services.Security;

namespace Grand.Framework.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring MVC on application startup
    /// </summary>
    public class GrandMvcStartup : IGrandStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //add MiniProfiler services
            services.AddMiniProfiler(options => {
                var memoryCache = EngineContext.Current.Resolve<IMemoryCache>();
                options.Storage = new StackExchange.Profiling.Storage.MemoryCacheStorage(memoryCache, TimeSpan.FromMinutes(60));
                //determine who can access the MiniProfiler results
                options.ResultsAuthorize = request =>
                    !EngineContext.Current.Resolve<StoreInformationSettings>().DisplayMiniProfilerInPublicStore ||
                    EngineContext.Current.Resolve<IPermissionService>().Authorize(StandardPermissionProvider.AccessAdminPanel);

            });

            //add and configure MVC feature
            services.AddGrandMvc();

            //add custom redirect result executor
            services.AddGrandRedirectResultExecutor();
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            //add MiniProfiler
            application.UseProfiler();

            //MVC routing
            application.UseGrandMvc();
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order
        {
            //MVC should be loaded last
            get { return 1000; }
        }
    }
}
