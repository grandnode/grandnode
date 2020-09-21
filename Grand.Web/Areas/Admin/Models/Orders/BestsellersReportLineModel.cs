using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class BestsellersReportLineModel : BaseModel
    {
        public string ProductId { get; set; }
        [GrandResourceDisplayName("Admin.Reports.Bestsellers.Fields.Name")]
        public string ProductName { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Bestsellers.Fields.TotalAmount")]
        public string TotalAmount { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Bestsellers.Fields.TotalQuantity")]
        public decimal TotalQuantity { get; set; }
    }
}