using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class BestCustomersReportModel : BaseGrandModel
    {
        public BestCustomersReportModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Customers.Reports.BestBy.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Reports.BestBy.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Reports.BestBy.OrderStatus")]
        public int OrderStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Customers.Reports.BestBy.PaymentStatus")]
        public int PaymentStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Customers.Reports.BestBy.ShippingStatus")]
        public int ShippingStatusId { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableShippingStatuses { get; set; }
    }
}