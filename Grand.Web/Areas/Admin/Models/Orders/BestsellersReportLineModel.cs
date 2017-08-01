using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class BestsellersReportLineModel : BaseGrandModel
    {
        public string ProductId { get; set; }
        [GrandResourceDisplayName("Admin.SalesReport.Bestsellers.Fields.Name")]
        public string ProductName { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Bestsellers.Fields.TotalAmount")]
        public string TotalAmount { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Bestsellers.Fields.TotalQuantity")]
        public decimal TotalQuantity { get; set; }
    }
}