using Grand.Core.Domain.Tasks;
using Grand.Core.Infrastructure;
using Microsoft.Extensions.DependencyModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Grand.Services.Tasks
{
    public class ScheduleTaskManager
    {
        private static readonly ScheduleTaskManager _manager = new ScheduleTaskManager();
        private static List<string> _runningTasks = new List<string>();
        private ScheduleTaskManager() { }
        public static ScheduleTaskManager Instance { get { return _manager; } }
        public static List<string> RunningTasks { get { return _runningTasks; } }

        public List<IScheduleTask> LoadScheduleTasks()
        {
            var scheduleTaskService = EngineContext.Current.Resolve<IScheduleTaskService>();
            var tasks = scheduleTaskService
                .GetAllTasks()
                .ToList();

            List<IScheduleTask> interfaceCollection = new List<IScheduleTask>();
            foreach (var task in tasks)
            {
                var _task = ChangeTypeToRightOne(task);
                if(_task!=null)
                    interfaceCollection.Add(_task);
            }
            return interfaceCollection;
        }

        public IScheduleTask ChangeTypeToRightOne(ScheduleTask scheduleTask)
        {
            
            IScheduleTask task = null;
            var type2 = System.Type.GetType(scheduleTask.Type);
            if (type2 != null)
            {
                object instance;
                instance = EngineContext.Current.Resolve(type2);
                task = instance as IScheduleTask;
            }
            if (task != null)
            {
                task.ScheduleTaskName = scheduleTask.ScheduleTaskName;
                task.Type = scheduleTask.Type;
                task.Enabled = scheduleTask.Enabled;
                task.StopOnError = scheduleTask.StopOnError;
                task.LastStartUtc = scheduleTask.LastStartUtc;
                task.LastNonSuccessEndUtc = scheduleTask.LastNonSuccessEndUtc;
                task.LastSuccessUtc = scheduleTask.LastSuccessUtc;
                task.TimeIntervalChoice = scheduleTask.TimeIntervalChoice;
                task.TimeInterval = scheduleTask.TimeInterval;
                task.MinuteOfHour = scheduleTask.MinuteOfHour;
                task.HourOfDay = scheduleTask.HourOfDay;
                task.DayOfWeek = scheduleTask.DayOfWeek;
                task.MonthOptionChoice = scheduleTask.MonthOptionChoice;
                task.DayOfMonth = scheduleTask.DayOfMonth;
            }
            return task;
        }
    }
}
