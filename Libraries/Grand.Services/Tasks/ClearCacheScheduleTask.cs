﻿using Grand.Core.Caching;
using Grand.Core.Domain.Tasks;
using Grand.Core.Infrastructure;
using Grand.Services.Logging;

namespace Grand.Services.Tasks
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
