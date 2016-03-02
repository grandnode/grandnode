using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Customers
{
    public enum CustomerActionConditionEnum
    {
        OneOfThem = 0,
        AllOfThem = 1,
    }

    public enum CustomerActionConditionTypeEnum
    {
        Product = 1,
        Category = 2,
        Manufacturer = 3,
        Vendor = 4,
        ProductAttribute = 5,
        ProductSpecification = 6,
        CustomerRole = 7,
        CustomerTag = 8,
        CustomerRegisterField = 9,
        UrlReferrer = 10,
        UrlCurrent = 11
    }

    public enum CustomerReactionTypeEnum
    {
        Banner = 1,
        Email = 2,
        AssignToCustomerRole = 3,
        AssignToCustomerTag = 4,            
    }
}
