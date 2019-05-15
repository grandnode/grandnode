using Grand.Core.Caching;
using Grand.Core.Infrastructure;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Clear cache schedueled task implementation
    /// </summary>
    public partial class ClearCacheScheduleTask : IScheduleTask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task Execute()
        {
            var cacheManagers = EngineContext.Current.ResolveAll<ICacheManager>();
            foreach (var cacheManager in cacheManagers)
            {
                await cacheManager.Clear();
            }
            await Task.CompletedTask;
        }
    }
}
