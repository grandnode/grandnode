using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;


namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class OrderPeriodReportLineModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.Reports.Period.Name")]
        public string Period { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Period.Count")]
        public int Count { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Period.Amount")]
        public decimal Amount { get; set; }

    }
}