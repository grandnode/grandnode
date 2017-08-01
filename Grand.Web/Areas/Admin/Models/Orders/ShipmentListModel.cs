using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class ShipmentListModel : BaseGrandModel
    {
        public ShipmentListModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Orders.Shipments.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.Orders.Shipments.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [GrandResourceDisplayName("Admin.Orders.Shipments.List.TrackingNumber")]
        
        public string TrackingNumber { get; set; }
        
        public IList<SelectListItem> AvailableCountries { get; set; }
        [GrandResourceDisplayName("Admin.Orders.Shipments.List.Country")]
        public string CountryId { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }
        [GrandResourceDisplayName("Admin.Orders.Shipments.List.StateProvince")]
        public int StateProvinceId { get; set; }

        [GrandResourceDisplayName("Admin.Orders.Shipments.List.City")]
        
        public string City { get; set; }

        [GrandResourceDisplayName("Admin.Orders.Shipments.List.LoadNotShipped")]
        public bool LoadNotShipped { get; set; }


        [GrandResourceDisplayName("Admin.Orders.Shipments.List.Warehouse")]
        public string WarehouseId { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }
    }
}