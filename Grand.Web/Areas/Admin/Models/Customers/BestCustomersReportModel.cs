﻿using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class BestCustomersReportModel : BaseModel
    {
        public BestCustomersReportModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Reports.Customers.BestBy.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Customers.BestBy.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Customers.BestBy.OrderStatus")]
        public int OrderStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Reports.Customers.BestBy.PaymentStatus")]
        public int PaymentStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Reports.Customers.BestBy.ShippingStatus")]
        public int ShippingStatusId { get; set; }

        public string StoreId { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableShippingStatuses { get; set; }
    }
}