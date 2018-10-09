using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class OrderListModel : BaseGrandModel
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

        [GrandResourceDisplayName("Admin.Orders.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.BillingEmail")]
        
        public string BillingEmail { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.BillingLastName")]
        
        public string BillingLastName { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.OrderStatus")]
        public int OrderStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Orders.List.PaymentStatus")]
        public int PaymentStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Orders.List.ShippingStatus")]
        public int ShippingStatusId { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.PaymentMethod")]
        public string PaymentMethodSystemName { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.Store")]
        public string StoreId { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.Vendor")]
        public string VendorId { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.Warehouse")]
        public string WarehouseId { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.Product")]
        public string ProductId { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.BillingCountry")]
        public string BillingCountryId { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.OrderNotes")]
        
        public string OrderNotes { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.OrderGuid")]
        
        public string OrderGuid { get; set; }

        [GrandResourceDisplayName("Admin.Orders.List.GoDirectlyToNumber")]
        
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