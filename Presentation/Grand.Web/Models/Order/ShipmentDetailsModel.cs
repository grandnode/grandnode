﻿using System;
using System.Collections.Generic;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Order
{
    public partial class ShipmentDetailsModel : BaseNopEntityModel
    {
        public ShipmentDetailsModel()
        {
            ShipmentStatusEvents = new List<ShipmentStatusEventModel>();
            Items = new List<ShipmentItemModel>();
        }
        public int ShipmentNumber { get; set; }
        public string TrackingNumber { get; set; }
        public string TrackingNumberUrl { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public IList<ShipmentStatusEventModel> ShipmentStatusEvents { get; set; }
        public bool ShowSku { get; set; }
        public IList<ShipmentItemModel> Items { get; set; }

        public OrderDetailsModel Order { get; set; }

		#region Nested Classes

        public partial class ShipmentItemModel : BaseNopEntityModel
        {
            public string Sku { get; set; }
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public string AttributeInfo { get; set; }
            public string RentalInfo { get; set; }

            public int QuantityOrdered { get; set; }
            public int QuantityShipped { get; set; }
        }

        public partial class ShipmentStatusEventModel : BaseNopModel
        {
            public string EventName { get; set; }
            public string Location { get; set; }
            public string Country { get; set; }
            public DateTime? Date { get; set; }
        }

		#endregion
    }
}