using Grand.Core.Data;
using Grand.Core.Domain.Tasks;
using Grand.Core.Infrastructure;
using Grand.Services.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
        /// <summary>
        public void Configure(IApplicationBuilder application)
        {

        }

        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //database is already installed, so start scheduled tasks
            if (DataSettingsHelper.DatabaseIsInstalled())
            {
                try
                {
                    var machineName = Environment.MachineName;
                    var tasks = new MongoDBRepository<ScheduleTask>(DataSettingsHelper.ConnectionString()).Table;
                    foreach (var task in tasks)
                    {
                        if (task.Enabled)
                        {
                            if (string.IsNullOrEmpty(task.LeasedByMachineName) || (machineName == task.LeasedByMachineName))
                            {
                                services.AddSingleton<IHostedService, BackgroundServiceTask>(sp =>
                                {
                                    return new BackgroundServiceTask(task, sp);
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
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
