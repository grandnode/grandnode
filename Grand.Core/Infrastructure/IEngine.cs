using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Grand.Core.Infrastructure
{
    /// <summary>
    /// Classes implementing this interface can serve as a portal for the 
    /// various services composing the Grand engine. Edit functionality, modules
    /// and implementations access most functionality through this 
    /// interface.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Initialize engine
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        void Initialize(IServiceCollection services);

        /// <summary>
        /// Add and configure services
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        /// <returns>Service provider</returns>
        IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration);

        /// <summary>
        /// Configure HTTP request pipeline
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        void ConfigureRequestPipeline(IApplicationBuilder application);

    }
}