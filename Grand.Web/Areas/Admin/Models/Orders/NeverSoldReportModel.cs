using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class NeverSoldReportModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.Reports.NeverSold.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.Reports.NeverSold.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }
    }
}