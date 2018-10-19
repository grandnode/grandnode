using Grand.Core.Domain.Tasks;
using System.Collections.Generic;

namespace Grand.Services.Tasks
{
    public partial interface IScheduleTaskService
    {
        /// <summary>
        /// Gets a task
        /// </summary>
        /// <param name="taskId">Task identifier</param>
        /// <returns>Task</returns>
        ScheduleTask GetTaskById(string taskId);

        /// <summary>
        /// Gets a task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        /// <returns>Task</returns>
        ScheduleTask GetTaskByType(string type);

        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <returns>Tasks</returns>
        IList<ScheduleTask> GetAllTasks();

        /// <summary>
        /// Updates the task
        /// </summary>
        /// <param name="task">Task</param>
        void UpdateTask(ScheduleTask task);
    }
}
