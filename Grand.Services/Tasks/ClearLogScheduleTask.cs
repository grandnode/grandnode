using Grand.Services.Logging;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Represents a task to clear [Log] table
    /// </summary>
    public partial class ClearLogScheduleTask : IScheduleTask
    {
        private readonly ILogger _logger;
        public ClearLogScheduleTask(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task Execute()
        {
            await _logger.ClearLog();
        }
    }
}
