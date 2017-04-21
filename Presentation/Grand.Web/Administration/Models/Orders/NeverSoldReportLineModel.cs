using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Orders
{
    public partial class NeverSoldReportLineModel : BaseGrandModel
    {
        public string ProductId { get; set; }
        [GrandResourceDisplayName("Admin.SalesReport.NeverSold.Fields.Name")]
        public string ProductName { get; set; }
    }
}