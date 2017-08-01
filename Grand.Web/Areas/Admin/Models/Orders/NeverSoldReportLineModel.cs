using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class NeverSoldReportLineModel : BaseGrandModel
    {
        public string ProductId { get; set; }
        [GrandResourceDisplayName("Admin.SalesReport.NeverSold.Fields.Name")]
        public string ProductName { get; set; }
    }
}