using Grand.Core.Domain.Tasks;
using Grand.Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;

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

            //change type of each task above from ScheduleTask into IScheduleTask
            //then, it makes it possible to do different code inside Execute() methods
            List<IScheduleTask> interfaceCollection = new List<IScheduleTask>();
            foreach (var task in tasks)
            {
                interfaceCollection.Add(ChangeTypeToRightOne(task));
            }
            return interfaceCollection;
        }

        public IScheduleTask ChangeTypeToRightOne(ScheduleTask scheduleTask)
        {
            //im sending general-type instance of ScheduleTask
            //because it is ScheduleTask - it doesn't consist useable Execute() method
            //I need 1 from 11 types, that have inside them useable and sensible executable code
            //this piece of code transforms ScheduleTask into Object, and then transforms it into IScheduleTask interface
            //but it doesn't consist any information - I need to assign it manually in #2 (see below)
            //and then - I have instances with 
            //right types
            //and
            //right values inside them
            //and
            //also magically all instances have already sent proper arguments into their constructors
            //and it is all that I need to life a fulfilled live 
            var scope = EngineContext.Current.ContainerManager.Scope();
            IScheduleTask task = null;
            var type2 = System.Type.GetType(scheduleTask.Type);
            if (type2 != null)
            {
                object instance;
                if (!EngineContext.Current.ContainerManager.TryResolve(type2, scope, out instance))
                {
                    //the type of object instance is determined by basing on "type2" variable
                    instance = EngineContext.Current.ContainerManager.ResolveUnregistered(type2, scope);
                    //now I have object with right type, but I need to change it to generic type - an interface
                }
                //here I change type to interface - it will work on every task that implements this interface thanks to polimorphism
                task = instance as IScheduleTask;
            }

            //#2 
            //the last thing to be done is to assign the right Property values to this instance
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
            return task;
        }
    }
}
