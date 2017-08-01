using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    public partial class ReportPeriodOrder
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}
