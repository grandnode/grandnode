﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Orders
{
    public partial class OrderListModel : BaseNopModel
    {
        public OrderListModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailablePaymentMethods = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Orders.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.BillingEmail")]
        [AllowHtml]
        public string BillingEmail { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.BillingLastName")]
        [AllowHtml]
        public string BillingLastName { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.OrderStatus")]
        public int OrderStatusId { get; set; }
        [NopResourceDisplayName("Admin.Orders.List.PaymentStatus")]
        public int PaymentStatusId { get; set; }
        [NopResourceDisplayName("Admin.Orders.List.ShippingStatus")]
        public int ShippingStatusId { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.PaymentMethod")]
        public string PaymentMethodSystemName { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.Store")]
        public string StoreId { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.Vendor")]
        public string VendorId { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.Warehouse")]
        public string WarehouseId { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.Product")]
        public string ProductId { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.BillingCountry")]
        public string BillingCountryId { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.OrderNotes")]
        [AllowHtml]
        public string OrderNotes { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.OrderGuid")]
        [AllowHtml]
        public string OrderGuid { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.GoDirectlyToNumber")]
        [AllowHtml]
        public int GoDirectlyToNumber { get; set; }

        public bool IsLoggedInAsVendor { get; set; }


        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableShippingStatuses { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }
        public IList<SelectListItem> AvailablePaymentMethods { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
    }
}