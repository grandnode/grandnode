using Grand.Services.Customers;
using Grand.Core.Domain.Tasks;

namespace Grand.Services.Tasks
{
    public partial class CustomerReminderUnpaidOrderScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICustomerReminderService _customerReminderService;

        public CustomerReminderUnpaidOrderScheduleTask(ICustomerReminderService customerReminderService)
        {
            this._customerReminderService = customerReminderService;
        }

        public void Execute()
        {
            _customerReminderService.Task_UnpaidOrder();
        }
    }
}
