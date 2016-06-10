using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Home
{
    public partial class DashboardModel : BaseNopModel
    {
        public bool IsLoggedInAsVendor { get; set; }

    }
    public partial class DashboardActivityModel : BaseNopModel
    {
        public int OrdersPending { get; set; }
        public int AbandonedCarts { get; set; }
        public int LowStockProducts { get; set; }
        public int TodayRegisteredCustomers { get; set; }
        public int ReturnRequests { get; set; }
    }
}