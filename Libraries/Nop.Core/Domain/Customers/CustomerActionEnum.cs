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
        ProductSpecification = 6
    }
}
