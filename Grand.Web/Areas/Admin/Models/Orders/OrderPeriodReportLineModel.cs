using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class OrderPeriodReportLineModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.SalesReport.Period.Name")]
        public string Period { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Period.Count")]
        public int Count { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Period.Amount")]
        public decimal Amount { get; set; }

    }
}