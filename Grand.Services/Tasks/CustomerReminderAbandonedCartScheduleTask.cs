using Grand.Services.Customers;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    public partial class CustomerReminderAbandonedCartScheduleTask : IScheduleTask
    {
        private readonly ICustomerReminderService _customerReminderService;

        public CustomerReminderAbandonedCartScheduleTask(ICustomerReminderService customerReminderService)
        {
            _customerReminderService = customerReminderService;
        }

        public async Task Execute()
        {
            await _customerReminderService.Task_AbandonedCart();
        }
    }
}
