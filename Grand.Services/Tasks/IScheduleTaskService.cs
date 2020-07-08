using Grand.Domain.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    public partial interface IScheduleTaskService
    {
        /// <summary>
        /// Gets a task
        /// </summary>
        /// <param name="taskId">Task identifier</param>
        /// <returns>Task</returns>
        Task<ScheduleTask> GetTaskById(string taskId);

        /// <summary>
        /// Gets a task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        /// <returns>Task</returns>
        Task<ScheduleTask> GetTaskByType(string type);

        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <returns>Tasks</returns>
        Task<IList<ScheduleTask>> GetAllTasks();

        /// <summary>
        /// Insert the task
        /// </summary>
        /// <param name="task">Task</param>
        Task<ScheduleTask> InsertTask(ScheduleTask task);

        /// <summary>
        /// Updates the task
        /// </summary>
        /// <param name="task">Task</param>
        Task UpdateTask(ScheduleTask task);

        /// <summary>
        /// Delete the task
        /// </summary>
        /// <param name="task">Task</param>
        Task DeleteTask(ScheduleTask task);
    }
}
