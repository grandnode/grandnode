using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class OrderAverageReportLineSummaryModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.SalesReport.Average.OrderStatus")]
        public string OrderStatus { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Average.SumTodayOrders")]
        public string SumTodayOrders { get; set; }
        
        [GrandResourceDisplayName("Admin.SalesReport.Average.SumThisWeekOrders")]
        public string SumThisWeekOrders { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Average.SumThisMonthOrders")]
        public string SumThisMonthOrders { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Average.SumThisYearOrders")]
        public string SumThisYearOrders { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Average.SumAllTimeOrders")]
        public string SumAllTimeOrders { get; set; }
    }
}
