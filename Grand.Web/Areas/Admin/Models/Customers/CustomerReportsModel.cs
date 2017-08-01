using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerReportsModel : BaseGrandModel
    {
        public BestCustomersReportModel BestCustomersByOrderTotal { get; set; }
        public BestCustomersReportModel BestCustomersByNumberOfOrders { get; set; }
    }
}