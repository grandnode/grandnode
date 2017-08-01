using Grand.Services.Customers;
using Grand.Core.Domain.Tasks;

namespace Grand.Services.Tasks
{
    public partial class CustomerReminderAbandonedCartScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICustomerReminderService _customerReminderService;

        public CustomerReminderAbandonedCartScheduleTask(ICustomerReminderService customerReminderService)
        {
            this._customerReminderService = customerReminderService;
        }

        public void Execute()
        {
            _customerReminderService.Task_AbandonedCart();
        }
    }
}
