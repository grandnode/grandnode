using Grand.Core.Caching;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Clear cache schedueled task implementation
    /// </summary>
    public partial class ClearCacheScheduleTask : IScheduleTask
    {
        private readonly ICacheBase _cacheBase;

        public ClearCacheScheduleTask(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task Execute()
        {
            await _cacheBase.Clear();
        }
    }
}
