using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class NeverSoldReportModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.SalesReport.NeverSold.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.NeverSold.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }
    }
}