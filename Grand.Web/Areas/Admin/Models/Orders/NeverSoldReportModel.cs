using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using Grand.Framework;
using Grand.Framework.Mvc;

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