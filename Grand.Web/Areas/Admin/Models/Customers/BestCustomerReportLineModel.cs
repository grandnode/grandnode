using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class BestCustomerReportLineModel : BaseGrandModel
    {
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.Customer")]
        public string CustomerName { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.OrderTotal")]
        public string OrderTotal { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.OrderCount")]
        public decimal OrderCount { get; set; }
    }
}