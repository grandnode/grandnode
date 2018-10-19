using Grand.Core.Domain.Tasks;
using Grand.Services.Customers;

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
