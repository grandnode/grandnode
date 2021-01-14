using Grand.Core.ModelBinding;
using Grand.Core.Models;


namespace Grand.Admin.Models.Orders
{
    public partial class OrderPeriodReportLineModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.Reports.Period.Name")]
        public string Period { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Period.Count")]
        public int Count { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Period.Amount")]
        public decimal Amount { get; set; }

    }
}