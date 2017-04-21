using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Orders
{
    public partial class CountryReportModel : BaseGrandModel
    {
        public CountryReportModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.SalesReport.Country.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.SalesReport.Country.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }


        [GrandResourceDisplayName("Admin.SalesReport.Country.OrderStatus")]
        public int OrderStatusId { get; set; }
        [GrandResourceDisplayName("Admin.SalesReport.Country.PaymentStatus")]
        public int PaymentStatusId { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
    }
}