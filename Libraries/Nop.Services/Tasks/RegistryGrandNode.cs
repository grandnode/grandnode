using System;
using System.Collections.Generic;
using System.Linq;
using FluentScheduler;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;
using Autofac;
using Nop.Core.Configuration;
using Nop.Services.Infrastructure;
using Nop.Core.Domain.Tasks;

namespace Nop.Services.Tasks
{
    public class RegistryGrandNode : Registry
    {
        public RegistryGrandNode(List<IScheduleTask> tasks, bool throwException = false)
        {
            foreach (var task in tasks.Where(x => x.Enabled))
            {
                switch (task.TimeIntervalChoice)
                {
                    #region choice cases
                    case TimeIntervalChoice.EVERY_MINUTES: 
                        {
                            Schedule(() => ExecuteTask(task)).ToRunEvery(task.TimeInterval).Minutes();
                            break;
                        }
                    case TimeIntervalChoice.EVERY_HOURS: 
                        {
                            Schedule(() => ExecuteTask(task)).ToRunEvery(task.TimeInterval).Hours().At(task.MinuteOfHour);
                            break;
                        }
                    case TimeIntervalChoice.EVERY_DAYS: 
                        {
                            Schedule(() => ExecuteTask(task)).ToRunEvery(task.TimeInterval).Days().At(task.HourOfDay, task.MinuteOfHour);
                            break;
                        }
                    case TimeIntervalChoice.EVERY_WEEKS:
                        {
                            Schedule(() => ExecuteTask(task))
                                .ToRunEvery(task.TimeInterval)
                                .Weeks()
                                .On(task.DayOfWeek)
                                .At(task.HourOfDay, task.MinuteOfHour);
                            break;
                        }
                    case TimeIntervalChoice.EVERY_MONTHS: 
                        {
                            switch (task.MonthOptionChoice)
                            {
                                case MonthOptionChoice.ON_SPECIFIC_DAY:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .On(task.DayOfMonth) //any week by specifing any day
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.ON_THE_FIRST_WEEK_OF_MONTH:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheFirst(task.DayOfWeek) //first week
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.ON_THE_SECOND_WEEK_OF_MONTH:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheSecond(task.DayOfWeek) //second week
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.ON_THE_THIRD_WEEK_OF_MONTH:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheThird(task.DayOfWeek) //third week
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.ON_THE_FOURTH_WEEK_OF_MONTH:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheFourth(task.DayOfWeek) //fourth week
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.ON_THE_LAST_WEEK_OF_MONTH:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheLast(task.DayOfWeek) //last week (only febuary with 28 days can have exactly 4 weeks)
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.ON_THE_LAST_DAY_OF_MONTH:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheLastDay() //last day of month
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                default:
                                    {
                                        throw new InvalidOperationException("Wrong enum value - value must be of 0,10,20 etc");
                                    }
                            }
                            break;
                        }
                    default:
                        {
                            throw new InvalidOperationException("Wrong enum value - value must be of 0,10,20 etc");
                        }
                        #endregion
                }
            }
        }

        private static void ExecuteTask(IScheduleTask scheduleTask, bool throwException = false, bool dispose = true, bool ensureRunOnOneWebFarmInstance = true)
        {
            var scope = EngineContext.Current.ContainerManager.Scope();
            try
            {

                //task is run on one farm node at a time?
                if (ensureRunOnOneWebFarmInstance) //ten kod odpowiada za odpalaanie tylko na 1 serwerze
                {
                    //is web farm enabled (multiple instances)?
                    var nopConfig = EngineContext.Current.ContainerManager.Resolve<NopConfig>("", scope);
                    if (nopConfig.MultipleInstancesEnabled)
                    {
                        var machineNameProvider = EngineContext.Current.ContainerManager.Resolve<IMachineNameProvider>("", scope);
                        var machineName = machineNameProvider.GetMachineName();
                        if (String.IsNullOrEmpty(machineName))
                        {
                            throw new Exception("Machine name cannot be detected. You cannot run in web farm.");
                            //actually in this case we can generate some unique string (e.g. Guid) and store it in some "static" (!!!) variable
                            //then it can be used as a machine name
                        }

                        //lease can't be aquired only if for a different machine and it has not expired
                        if (scheduleTask.LeasedUntilUtc.HasValue &&
                            scheduleTask.LeasedUntilUtc.Value >= DateTime.UtcNow &&
                            scheduleTask.LeasedByMachineName != machineName)
                            return;

                        //lease the task. so it's run on one farm node at a time
                        scheduleTask.LeasedByMachineName = machineName;
                        scheduleTask.LeasedUntilUtc = DateTime.UtcNow.AddMinutes(30);
                    }
                }

                scheduleTask.LastStartUtc = DateTime.UtcNow;
                scheduleTask.Execute();
                scheduleTask.LastSuccessUtc = DateTime.UtcNow;
                scheduleTask.LastNonSuccessEndUtc = null;
            }
            catch (Exception exc)
            {
                scheduleTask.Enabled = !scheduleTask.StopOnError;
                scheduleTask.LastNonSuccessEndUtc = DateTime.UtcNow;
                scheduleTask.LastSuccessUtc = null;

                //log error
                var logger = EngineContext.Current.ContainerManager.Resolve<ILogger>("", scope);
                logger.Error(string.Format("Error while running the '{0}' schedule task. {1}", scheduleTask.ScheduleTaskName, exc.Message), exc);
                if (throwException)
                    throw;
            }
            finally
            {
                var scheduleTaskService = EngineContext.Current.Resolve<IScheduleTaskService>();
                var taskToUpdate = scheduleTaskService.GetTaskByType(scheduleTask.Type);

                taskToUpdate.Enabled = scheduleTask.Enabled;
                taskToUpdate.LastStartUtc = scheduleTask.LastStartUtc;
                taskToUpdate.LastNonSuccessEndUtc = scheduleTask.LastNonSuccessEndUtc;
                taskToUpdate.LastSuccessUtc = scheduleTask.LastSuccessUtc;
                taskToUpdate.LeasedByMachineName = scheduleTask.LeasedByMachineName;
                taskToUpdate.LeasedUntilUtc = scheduleTask.LeasedUntilUtc;
                scheduleTaskService.UpdateTask(taskToUpdate);

                //dispose all resources
                if (dispose)
                {
                    scope.Dispose();
                }
            }
        }

        public static void RunTaskNow(ScheduleTask scheduleTask)
        {
            //elegant in its simplicity 
            var scheduleTask02 = ScheduleTaskManager.Instance.ChangeTypeToRightOne(scheduleTask);
            ExecuteTask(scheduleTask02);
        }
    }
}
