using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;


namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class OrderPeriodReportLineModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.SalesReport.Period.Name")]
        public string Period { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Period.Count")]
        public int Count { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Period.Amount")]
        public decimal Amount { get; set; }

    }
}