using Nop.Core.Caching;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;

namespace Nop.Services.Tasks
{
    /// <summary>
    /// Clear cache schedueled task implementation
    /// </summary>
    public partial class ClearCacheScheduleTask : ScheduleTask, IScheduleTask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        /// 
        public ClearCacheScheduleTask() { }

        public void Execute()
        {
            var cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>();
            cacheManager.Clear();
        }
    }
}
