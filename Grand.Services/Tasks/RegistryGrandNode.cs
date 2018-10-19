using Autofac;
using FluentScheduler;
using Grand.Core.Configuration;
using Grand.Core.Domain.Tasks;
using Grand.Core.Infrastructure;
using Grand.Services.Infrastructure;
using Grand.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Tasks
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
                    case TimeIntervalChoice.EveryMinutes: 
                        {
                            Schedule(() => ExecuteTask(task)).ToRunEvery(task.TimeInterval).Minutes();
                            break;
                        }
                    case TimeIntervalChoice.EveryHours: 
                        {
                            Schedule(() => ExecuteTask(task)).ToRunEvery(task.TimeInterval).Hours().At(task.MinuteOfHour);
                            break;
                        }
                    case TimeIntervalChoice.EveryDays: 
                        {
                            Schedule(() => ExecuteTask(task)).ToRunEvery(task.TimeInterval).Days().At(task.HourOfDay, task.MinuteOfHour);
                            break;
                        }
                    case TimeIntervalChoice.EveryWeeks:
                        {
                            Schedule(() => ExecuteTask(task))
                                .ToRunEvery(task.TimeInterval)
                                .Weeks()
                                .On(task.DayOfWeek)
                                .At(task.HourOfDay, task.MinuteOfHour);
                            break;
                        }
                    case TimeIntervalChoice.EveryMonths: 
                        {
                            switch (task.MonthOptionChoice)
                            {
                                case MonthOptionChoice.OnSpecificDay:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .On(task.DayOfMonth) //any week by specifing any day
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.OnTheFirstWeekOfMonth:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheFirst(task.DayOfWeek) //first week
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.OnTheSecondWeekOfMonth:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheSecond(task.DayOfWeek) //second week
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.OnTheThirdWeekOfMonth:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheThird(task.DayOfWeek) //third week
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.OnTheFourtWeekOfMonth:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheFourth(task.DayOfWeek) //fourth week
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.OnTheLastWeekOfMonth:
                                    {
                                        Schedule(() => ExecuteTask(task))
                                            .ToRunEvery(task.TimeInterval)
                                            .Months()
                                            .OnTheLast(task.DayOfWeek) //last week (only febuary with 28 days can have exactly 4 weeks)
                                            .At(task.HourOfDay, task.MinuteOfHour);
                                        break;
                                    }
                                case MonthOptionChoice.OnTheLastDayOfMonth:
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

        private static void ExecuteTask(IScheduleTask scheduleTask, bool manualstart = false)
        {
            try
            {
                if (!scheduleTask.Enabled && !manualstart)
                    return;

                bool runTask = true;
                //is web farm enabled (multiple instances)?
                var grandConfig = EngineContext.Current.Resolve<GrandConfig>();
                if (grandConfig.MultipleInstancesEnabled)
                {
                    var machineNameProvider = EngineContext.Current.Resolve<IMachineNameProvider>();
                    var machineName = machineNameProvider.GetMachineName();
                    if (String.IsNullOrEmpty(machineName))
                    {
                        throw new Exception("Machine name cannot be detected. You cannot run in web farm.");
                        //actually in this case we can generate some unique string (e.g. Guid) and store it in some "static" (!!!) variable
                        //then it can be used as a machine name
                    }

                    if (!string.IsNullOrEmpty(scheduleTask.LeasedByMachineName) && (machineName != scheduleTask.LeasedByMachineName))
                    {
                        runTask = false;
                    }
                }
                if (runTask == true)
                {
                    scheduleTask.LastStartUtc = DateTime.UtcNow;
                    scheduleTask.Execute();
                    scheduleTask.LastSuccessUtc = DateTime.UtcNow;
                    scheduleTask.LastNonSuccessEndUtc = null;
                }
            }
            catch (Exception exc)
            {
                scheduleTask.Enabled = !scheduleTask.StopOnError;
                scheduleTask.LastNonSuccessEndUtc = DateTime.UtcNow;
                scheduleTask.LastSuccessUtc = null;

                //log error
                var logger = EngineContext.Current.Resolve<ILogger>();
                logger.Error(string.Format("Error while running the '{0}' schedule task. {1}", scheduleTask.ScheduleTaskName, exc.Message), exc);
            }
            finally
            {
                var scheduleTaskService = EngineContext.Current.Resolve<IScheduleTaskService>();
                var taskToUpdate = scheduleTaskService.GetTaskByType(scheduleTask.Type);

                taskToUpdate.Enabled = scheduleTask.Enabled;
                taskToUpdate.LastStartUtc = scheduleTask.LastStartUtc;
                taskToUpdate.LastNonSuccessEndUtc = scheduleTask.LastNonSuccessEndUtc;
                taskToUpdate.LastSuccessUtc = scheduleTask.LastSuccessUtc;
                taskToUpdate.LeasedUntilUtc = scheduleTask.LeasedUntilUtc;
                scheduleTaskService.UpdateTask(taskToUpdate);

            }
        }

        public static void RunTaskNow(ScheduleTask scheduleTask)
        {
            var task = ScheduleTaskManager.Instance.ChangeType(scheduleTask);
            if(task!=null)
                ExecuteTask(task, true);
        }
    }
}
