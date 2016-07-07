using System.Net;
using Nop.Core;
using Nop.Services.Tasks;
using Nop.Core.Domain.Tasks;

namespace Nop.Services.Tasks
{
    /// <summary>
    /// Represents a task for keeping the site alive
    /// </summary>
    public partial class KeepAliveScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly IStoreContext _storeContext;

        public KeepAliveScheduleTask(IStoreContext storeContext)
        {
            this._storeContext = storeContext;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            string url = _storeContext.CurrentStore.Url + "keepalive/index";
            using (var wc = new WebClient())
            {
                wc.DownloadString(url);
            }
        }
    }
}
