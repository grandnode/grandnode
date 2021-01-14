using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Orders
{
    public partial class NeverSoldReportLineModel : BaseModel
    {
        public string ProductId { get; set; }
        [GrandResourceDisplayName("Admin.Reports.NeverSold.Fields.Name")]
        public string ProductName { get; set; }
    }
}