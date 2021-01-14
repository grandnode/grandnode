using Grand.Core.Models;

namespace Grand.Admin.Models.Home
{
    public partial class DashboardModel : BaseModel
    {
        public bool IsLoggedInAsVendor { get; set; }
        public bool HideReportGA { get; set; }

    }
    public partial class DashboardActivityModel : BaseModel
    {
        public int OrdersPending { get; set; }
        public int AbandonedCarts { get; set; }
        public int LowStockProducts { get; set; }
        public int TodayRegisteredCustomers { get; set; }
        public int ReturnRequests { get; set; }
    }
}