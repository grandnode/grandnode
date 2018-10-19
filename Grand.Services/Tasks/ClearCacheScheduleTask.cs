using Grand.Core.Caching;
using Grand.Core.Domain.Tasks;
using Grand.Core.Infrastructure;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Clear cache schedueled task implementation
    /// </summary>
    public partial class ClearCacheScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly object _lock = new object();
        
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            lock (_lock)
            {
                var cacheManager = EngineContext.Current.Resolve<ICacheManager>();
                cacheManager.Clear();
            }
        }
    }
}
