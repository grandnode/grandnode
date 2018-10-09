using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerReportsModel : BaseGrandModel
    {
        public BestCustomersReportModel BestCustomersByOrderTotal { get; set; }
        public BestCustomersReportModel BestCustomersByNumberOfOrders { get; set; }
    }
}