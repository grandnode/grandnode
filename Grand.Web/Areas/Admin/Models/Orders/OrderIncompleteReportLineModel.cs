using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class OrderIncompleteReportLineModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.SalesReport.Incomplete.Item")]
        public string Item { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Incomplete.Total")]
        public string Total { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Incomplete.Count")]
        public int Count { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Incomplete.View")]
        public string ViewLink { get; set; }
    }
}
