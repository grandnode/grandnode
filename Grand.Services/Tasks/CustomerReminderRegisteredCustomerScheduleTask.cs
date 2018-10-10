using Grand.Core.Domain.Tasks;
using Grand.Services.Customers;

namespace Grand.Services.Tasks
{
    public partial class CustomerReminderRegisteredCustomerScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICustomerReminderService _customerReminderService;
        private readonly object _lock = new object();
        public CustomerReminderRegisteredCustomerScheduleTask(ICustomerReminderService customerReminderService)
        {
            this._customerReminderService = customerReminderService;
        }

        public void Execute()
        {
            lock (_lock)
            {
                _customerReminderService.Task_RegisteredCustomer();
            }
        }
    }
}
