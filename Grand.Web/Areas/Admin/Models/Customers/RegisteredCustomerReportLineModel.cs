using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class RegisteredCustomerReportLineModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.Reports.Customers.RegisteredCustomers.Fields.Period")]
        public string Period { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Customers.RegisteredCustomers.Fields.Customers")]
        public int Customers { get; set; }
    }
}