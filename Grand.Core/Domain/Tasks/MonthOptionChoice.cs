using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Core.Domain.Tasks
{
    public enum MonthOptionChoice
    {
        ON_SPECIFIC_DAY = 10,
        ON_THE_FIRST_WEEK_OF_MONTH = 20,
        ON_THE_SECOND_WEEK_OF_MONTH = 30,
        ON_THE_THIRD_WEEK_OF_MONTH = 40,
        ON_THE_FOURTH_WEEK_OF_MONTH = 50,
        ON_THE_LAST_WEEK_OF_MONTH = 60,
        ON_THE_LAST_DAY_OF_MONTH = 70
    }
}
