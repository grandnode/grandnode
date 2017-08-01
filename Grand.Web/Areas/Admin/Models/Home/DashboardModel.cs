using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Home
{
    public partial class DashboardModel : BaseGrandModel
    {
        public bool IsLoggedInAsVendor { get; set; }
        public bool HideReportGA { get; set; }

    }
    public partial class DashboardActivityModel : BaseGrandModel
    {
        public int OrdersPending { get; set; }
        public int AbandonedCarts { get; set; }
        public int LowStockProducts { get; set; }
        public int TodayRegisteredCustomers { get; set; }
        public int ReturnRequests { get; set; }
    }
}