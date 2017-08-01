using System.Net;
using Grand.Core;
using Grand.Services.Tasks;
using Grand.Core.Domain.Tasks;
using System.Net.Http;

namespace Grand.Services.Tasks
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
            using (var wc = new HttpClient())
            {
                wc.GetStringAsync(url);
            }
        }
    }
}
