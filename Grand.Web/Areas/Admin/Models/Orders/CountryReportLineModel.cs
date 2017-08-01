using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class CountryReportLineModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.SalesReport.Country.Fields.CountryName")]
        public string CountryName { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Country.Fields.TotalOrders")]
        public int TotalOrders { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Country.Fields.SumOrders")]
        public string SumOrders { get; set; }
    }
}