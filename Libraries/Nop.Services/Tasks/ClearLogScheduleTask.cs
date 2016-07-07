using Nop.Core.Caching;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Tasks;

namespace Nop.Services.Tasks
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
