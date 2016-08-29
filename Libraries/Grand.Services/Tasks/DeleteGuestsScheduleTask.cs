﻿using System;
using Grand.Services.Customers;
using Grand.Core.Domain.Tasks;
using Grand.Core.Domain.Common;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Represents a task for deleting guest customers
    /// </summary>
    public partial class DeleteGuestsScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICustomerService _customerService;
        private readonly CommonSettings _commonSettings;

        public DeleteGuestsScheduleTask(ICustomerService customerService, CommonSettings commonSettings)
        {
            this._customerService = customerService;
            this._commonSettings = commonSettings;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var olderThanMinutes = _commonSettings.DeleteGuestTaskOlderThanMinutes;
            // Default value in case 0 is returned.  0 would effectively disable this service and harm performance.
            olderThanMinutes = olderThanMinutes == 0 ? 1440 : olderThanMinutes;
            _customerService.DeleteGuestCustomers(null, DateTime.UtcNow.AddMinutes(-olderThanMinutes), true);
        }
    }
}
