using Grand.Core.Caching;
using Grand.Core.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Clear cache schedueled task implementation
    /// </summary>
    public partial class ClearCacheScheduleTask : IScheduleTask
    {
        private readonly IEnumerable<ICacheManager> _cacheManager;

        public ClearCacheScheduleTask(IEnumerable<ICacheManager> cacheManager)
        {
            _cacheManager = cacheManager;
        }
        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task Execute()
        {
            foreach (var cacheManager in _cacheManager)
            {
                await cacheManager.Clear();
            }
            await Task.CompletedTask;
        }
    }
}
