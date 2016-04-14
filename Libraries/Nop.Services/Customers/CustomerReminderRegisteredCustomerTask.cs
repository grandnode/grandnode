using Nop.Services.Logging;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Customers
{
    public partial class CustomerReminderRegisteredCustomerTask : ITask
    {
        private readonly ICustomerReminderService _customerReminderService;

        public CustomerReminderRegisteredCustomerTask(ICustomerReminderService customerReminderService)
        {
            this._customerReminderService = customerReminderService;
        }

        public virtual void Execute()
        { 
            _customerReminderService.Task_RegisteredCustomer();
        }
    }
}
