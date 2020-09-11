using Grand.Services.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    public class BackgroundServiceTask : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private string _taskType;
        public BackgroundServiceTask(string tasktype, IServiceProvider serviceProvider)
        {
            _taskType = tasktype;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var serviceProvider = scope.ServiceProvider;
                    var logger = serviceProvider.GetService<ILogger>();
                    var scheduleTaskService = serviceProvider.GetService<IScheduleTaskService>();
                    var task = await scheduleTaskService.GetTaskByType(_taskType);
                    if (task == null)
                    {
                        logger.Information($"Task {_taskType} is not exists in the database");
                        break;
                    }

                    var machineName = Environment.MachineName;
                    var timeInterval = task.TimeInterval > 0 ? task.TimeInterval : 1;
                    if (task.Enabled && (string.IsNullOrEmpty(task.LeasedByMachineName) || (machineName == task.LeasedByMachineName)))
                    {
                        var typeofTask = Type.GetType(_taskType);
                        if (typeofTask != null)
                        {
                            var scheduleTask = serviceProvider.GetServices<IScheduleTask>().FirstOrDefault(x => x.GetType() == typeofTask);
                            if (scheduleTask != null)
                            {
                                task.LastStartUtc = DateTime.UtcNow;
                                try
                                {
                                    await scheduleTask.Execute();
                                    task.LastSuccessUtc = DateTime.UtcNow;
                                    task.LastNonSuccessEndUtc = null;
                                }
                                catch (Exception exc)
                                {
                                    task.LastNonSuccessEndUtc = DateTime.UtcNow;
                                    task.Enabled = !task.StopOnError;
                                    await logger.InsertLog(Domain.Logging.LogLevel.Error, $"Error while running the '{task.ScheduleTaskName}' schedule task", exc.Message);
                                }
                            }
                            else
                            {
                                task.Enabled = !task.StopOnError;
                                task.LastNonSuccessEndUtc = DateTime.UtcNow;
                                await logger.InsertLog(Domain.Logging.LogLevel.Error, $"Type {_taskType} is not registered");
                            }
                        }
                        else
                        {
                            task.Enabled = !task.StopOnError;
                            task.LastNonSuccessEndUtc = DateTime.UtcNow;
                            await logger.InsertLog(Domain.Logging.LogLevel.Error, $"Type {_taskType} is null (type not exists)");
                        }
                        await scheduleTaskService.UpdateTask(task);
                        await Task.Delay(TimeSpan.FromMinutes(timeInterval), stoppingToken);
                    }
                    else
                        break;

                }
                catch(Exception ex)
                {
                    Serilog.Log.Logger.Error(ex, "BackgroundServiceTask");
                }


            }
        }
    }
}
