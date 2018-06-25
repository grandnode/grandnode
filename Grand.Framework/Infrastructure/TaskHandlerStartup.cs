using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Grand.Core.Infrastructure;
using Grand.Core.Data;
using Grand.Services.Logging;
using FluentScheduler;
using Grand.Services.Tasks;
using System;

namespace Grand.Framework.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring task on application startup
    /// </summary>
    public class TaskHandlerStartup : IGrandStartup
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
        public void Configure(IApplicationBuilder application)
        {
            if (DataSettingsHelper.DatabaseIsInstalled())
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                //database is already installed, so start scheduled tasks
                try
                {
                    JobManager.UseUtcTime();
                    JobManager.JobException += info => logger.Fatal("An error just happened with a scheduled job: " + info.Exception);
                    var scheduleTasks = ScheduleTaskManager.Instance.LoadScheduleTasks();       //load records from db to collection
                    JobManager.Initialize(new RegistryGrandNode(scheduleTasks));                //init registry and start scheduled tasks
                }
                catch (Exception ex)
                {
                    logger.Fatal("Application started error", ex, null);
                }
            }


        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order
        {
            //task handlers should be loaded last
            get { return 500; }
        }
    }
}
