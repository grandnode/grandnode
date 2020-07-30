using Grand.Domain.Data;
using Grand.Domain.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Task service
    /// </summary>
    public class ScheduleTaskService : IScheduleTaskService
    {
        #region Fields

        private readonly IRepository<ScheduleTask> _taskRepository;

        #endregion

        #region Ctor

        public ScheduleTaskService(IRepository<ScheduleTask> taskRepository)
        {
            _taskRepository = taskRepository;
        }

        #endregion

        /// <summary>
        /// Gets a task
        /// </summary>
        /// <param name="taskId">Task identifier</param>
        /// <returns>Task</returns>
        public virtual Task<ScheduleTask> GetTaskById(string taskId)
        {
            return _taskRepository.GetByIdAsync(taskId);
        }

        /// <summary>
        /// Gets a task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        /// <returns>Task</returns>
        public virtual Task<ScheduleTask> GetTaskByType(string type)
        {
            if (String.IsNullOrWhiteSpace(type))
                return Task.FromResult<ScheduleTask>(null);

            var query = _taskRepository.Table;

            query = query.Where(st => st.Type == type);
            query = query.OrderByDescending(t => t.Id);

            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Tasks</returns>
        public virtual async Task<IList<ScheduleTask>> GetAllTasks()
        {
            return await _taskRepository.Table.ToListAsync();
        }

        /// <summary>
        /// Insert the task
        /// </summary>
        /// <param name="task">Task</param>
        public virtual async Task<ScheduleTask> InsertTask(ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            return await _taskRepository.InsertAsync(task);
        }

        /// <summary>
        /// Updates the task
        /// </summary>
        /// <param name="task">Task</param>
        public virtual async Task UpdateTask(ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            await _taskRepository.UpdateAsync(task);
        }

        /// <summary>
        /// Delete the task
        /// </summary>
        /// <param name="task">Task</param>
        public virtual async Task DeleteTask(ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            await _taskRepository.DeleteAsync(task);
        }
    }
}

