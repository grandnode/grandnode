using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Customers
{
    public partial class CustomerReportsModel : BaseNopModel
    {
        public BestCustomersReportModel BestCustomersByOrderTotal { get; set; }
        public BestCustomersReportModel BestCustomersByNumberOfOrders { get; set; }
    }
}