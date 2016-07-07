using Nop.Services.Logging;
using Nop.Services.Customers;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Core.Domain.Tasks;

namespace Nop.Services.Tasks
{
    public partial class CustomerReminderBirthdayScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICustomerReminderService _customerReminderService;

        public CustomerReminderBirthdayScheduleTask(ICustomerReminderService customerReminderService)
        {
            this._customerReminderService = customerReminderService;
        }

        public void Execute()
        {
            _customerReminderService.Task_Birthday();
        }
    }
}
