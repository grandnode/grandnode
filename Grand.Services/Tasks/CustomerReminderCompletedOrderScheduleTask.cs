using Grand.Services.Customers;
using Grand.Core.Domain.Tasks;

namespace Grand.Services.Tasks
{
    public partial class CustomerReminderCompletedOrderScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICustomerReminderService _customerReminderService;

        public CustomerReminderCompletedOrderScheduleTask(ICustomerReminderService customerReminderService)
        {
            this._customerReminderService = customerReminderService;
        }

        public void Execute()
        {
            _customerReminderService.Task_CompletedOrder();
        }
    }
}
