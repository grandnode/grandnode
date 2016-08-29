﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Core.Domain.Customers
{
    public enum CustomerReminderRuleEnum
    {
        AbandonedCart = 1,
        RegisteredCustomer = 2,
        LastPurchase = 3,
        LastActivity = 4,
        Birthday = 5
    }

    public enum CustomerReminderConditionTypeEnum
    {
        Product = 1,
        Category = 2,
        Manufacturer = 3,
        CustomerRole = 4,
        CustomerTag = 5,
        CustomerRegisterField = 6,
        CustomCustomerAttribute = 7,
    }

    public enum CustomerReminderConditionEnum
    {
        OneOfThem = 0,
        AllOfThem = 1,
    }
    public enum CustomerReminderHistoryStatusEnum
    {
        Started = 10,
        CompletedReminder = 20,
        CompletedOrdered = 30,
    }


}
