using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class NeverSoldReportLineModel : BaseGrandModel
    {
        public string ProductId { get; set; }
        [GrandResourceDisplayName("Admin.SalesReport.NeverSold.Fields.Name")]
        public string ProductName { get; set; }
    }
}