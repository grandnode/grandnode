using Grand.Domain.Common;
using Grand.Services.Customers;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Represents a task for deleting guest customers
    /// </summary>
    public partial class DeleteGuestsScheduleTask : IScheduleTask
    {
        private readonly ICustomerService _customerService;
        private readonly CommonSettings _commonSettings;

        public DeleteGuestsScheduleTask(ICustomerService customerService, CommonSettings commonSettings)
        {
            _customerService = customerService;
            _commonSettings = commonSettings;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task Execute()
        {
            var olderThanMinutes = _commonSettings.DeleteGuestTaskOlderThanMinutes;
            // Default value in case 0 is returned.  0 would effectively disable this service and harm performance.
            olderThanMinutes = olderThanMinutes == 0 ? 1440 : olderThanMinutes;
            await _customerService.DeleteGuestCustomers(null, DateTime.UtcNow.AddMinutes(-olderThanMinutes), true);
        }
    }
}
