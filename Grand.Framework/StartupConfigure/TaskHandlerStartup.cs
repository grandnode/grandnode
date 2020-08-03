using Grand.Core.Data;
using Grand.Core.Extensions;
using Grand.Core.Infrastructure;
using Grand.Core.Plugins;
using Grand.Services.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Grand.Framework.StartupConfigure
{
    /// <summary>
    /// Represents object for the configuring task on application startup
    /// </summary>
    public class TaskHandlerStartup : IGrandStartup
    {
        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        /// <param name="webHostEnvironment">WebHostEnvironment</param>
        public void Configure(IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {

        }

        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //database is already installed, so start scheduled tasks
            if (DataSettingsHelper.DatabaseIsInstalled())
            {
                var typeFinder = new WebAppTypeFinder();
                var scheduleTasks = typeFinder.FindClassesOfType<IScheduleTask>();

                var scheduleTasksInstalled = scheduleTasks
                .Where(t => PluginManager.FindPlugin(t).Return(plugin => plugin.Installed, true)); //ignore not installed plugins

                foreach (var task in scheduleTasksInstalled)
                {
                    var assemblyName = task.Assembly.GetName().Name;
                    services.AddSingleton<IHostedService, BackgroundServiceTask>(sp =>
                    {
                        return new BackgroundServiceTask($"{task.FullName}, {assemblyName}", sp);
                    });
                }
            }
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order {
            //task handlers should be loaded last
            get { return 1010; }
        }
    }
}
