using Grand.Services.Customers;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    public partial class CustomerReminderRegisteredCustomerScheduleTask : IScheduleTask
    {
        private readonly ICustomerReminderService _customerReminderService;
        public CustomerReminderRegisteredCustomerScheduleTask(ICustomerReminderService customerReminderService)
        {
            _customerReminderService = customerReminderService;
        }

        public async Task Execute()
        {
            await _customerReminderService.Task_RegisteredCustomer();
        }
    }
}
