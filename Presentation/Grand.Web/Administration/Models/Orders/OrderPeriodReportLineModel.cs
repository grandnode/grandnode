using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Grand.Admin.Models.Orders
{
    public partial class OrderPeriodReportLineModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.SalesReport.Period.Name")]
        public string Period { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Period.Count")]
        public int Count { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Period.Amount")]
        public decimal Amount { get; set; }

    }
}