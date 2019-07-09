using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class CountryReportLineModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.Reports.Country.Fields.CountryName")]
        public string CountryName { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Country.Fields.TotalOrders")]
        public int TotalOrders { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Country.Fields.SumOrders")]
        public string SumOrders { get; set; }
    }
}