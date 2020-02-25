using Grand.Core.Caching;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Clear cache schedueled task implementation
    /// </summary>
    public partial class ClearCacheScheduleTask : IScheduleTask
    {
        private readonly ICacheManager _cacheManager;

        public ClearCacheScheduleTask(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task Execute()
        {
            await _cacheManager.Clear();
        }
    }
}
