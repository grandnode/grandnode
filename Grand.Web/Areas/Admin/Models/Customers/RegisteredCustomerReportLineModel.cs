using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class RegisteredCustomerReportLineModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.Customers.Reports.RegisteredCustomers.Fields.Period")]
        public string Period { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Reports.RegisteredCustomers.Fields.Customers")]
        public int Customers { get; set; }
    }
}