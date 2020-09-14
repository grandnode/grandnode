using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerReportsModel : BaseModel
    {
        public BestCustomersReportModel BestCustomersByOrderTotal { get; set; }
        public BestCustomersReportModel BestCustomersByNumberOfOrders { get; set; }
    }
}