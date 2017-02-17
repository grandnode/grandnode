using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Orders
{
    public partial class OrderIncompleteReportLineModel : BaseNopModel
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
