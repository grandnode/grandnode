using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Orders
{
    public partial class ShipmentDetailsModel : BaseGrandEntityModel
    {
        public ShipmentDetailsModel()
        {
            ShipmentStatusEvents = new List<ShipmentStatusEventModel>();
            Items = new List<ShipmentItemModel>();
            ShipmentNotes = new List<ShipmentNote>();
        }
        public int ShipmentNumber { get; set; }
        public string TrackingNumber { get; set; }
        public string TrackingNumberUrl { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public IList<ShipmentStatusEventModel> ShipmentStatusEvents { get; set; }
        public bool ShowSku { get; set; }
        public IList<ShipmentItemModel> Items { get; set; }
        public IList<ShipmentNote> ShipmentNotes { get; set; }
        public OrderModel Order { get; set; }

		#region Nested Classes

        public partial class ShipmentItemModel : BaseGrandEntityModel
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

        public partial class ShipmentStatusEventModel : BaseGrandModel
        {
            public string EventName { get; set; }
            public string Location { get; set; }
            public string Country { get; set; }
            public DateTime? Date { get; set; }
        }
        public partial class ShipmentNote : BaseGrandEntityModel
        {
            public bool HasDownload { get; set; }
            public string Note { get; set; }
            public DateTime CreatedOn { get; set; }
            public string ShipmentId { get; set; }
        }
        public partial class OrderModel : BaseGrandEntityModel
        {
            public int OrderNumber { get; set; }
            public string OrderCode { get; set; }
            public string ShippingMethod { get; set; }
            public bool PickUpInStore { get; set; }
            public AddressModel PickupAddress { get; set; }
            public AddressModel ShippingAddress { get; set; }
        }
        #endregion
    }
}