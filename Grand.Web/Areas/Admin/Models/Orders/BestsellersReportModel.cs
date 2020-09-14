﻿using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class BestsellersReportModel : BaseModel
    {
        public BestsellersReportModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();

        }

        [GrandResourceDisplayName("Admin.Reports.Bestsellers.Store")]
        public string StoreId { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Bestsellers.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Bestsellers.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }


        [GrandResourceDisplayName("Admin.Reports.Bestsellers.OrderStatus")]
        public int OrderStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Reports.Bestsellers.PaymentStatus")]
        public int PaymentStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Reports.Bestsellers.BillingCountry")]
        public string BillingCountryId { get; set; }

        [GrandResourceDisplayName("Admin.Reports.Bestsellers.Vendor")]
        public string VendorId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }
        public bool IsLoggedInAsVendor { get; set; }
    }
}