using Grand.Services.Logging;
using Grand.Services.Customers;
using Grand.Core.Caching;
using Grand.Core.Infrastructure;
using Grand.Core.Domain.Tasks;

namespace Grand.Services.Tasks
{
    public partial class CustomerReminderBirthdayScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICustomerReminderService _customerReminderService;
        private readonly object _lock = new object();

        public CustomerReminderBirthdayScheduleTask(ICustomerReminderService customerReminderService)
        {
            this._customerReminderService = customerReminderService;
        }

        public void Execute()
        {
            lock (_lock)
            {
                _customerReminderService.Task_Birthday();
            }
        }
    }
}
