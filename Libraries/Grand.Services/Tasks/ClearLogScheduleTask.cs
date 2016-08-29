using Grand.Core.Caching;
using Grand.Core.Domain.Tasks;
using Grand.Core.Infrastructure;
using Grand.Services.Logging;
using Grand.Services.Tasks;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Represents a task to clear [Log] table
    /// </summary>
    public partial class ClearLogScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ILogger _logger;

        public ClearLogScheduleTask(ILogger logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            _logger.ClearLog();
        }
    }
}
