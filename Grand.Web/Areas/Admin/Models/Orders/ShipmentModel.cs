using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class ShipmentModel : BaseGrandEntityModel
    {
        public ShipmentModel()
        {
            ShipmentStatusEvents = new List<ShipmentStatusEventModel>();
            Items = new List<ShipmentItemModel>();
        }
        [GrandResourceDisplayName("Admin.Orders.Shipments.ID")]
        public override string Id { get; set; }
        public int ShipmentNumber { get; set; }

        [GrandResourceDisplayName("Admin.Orders.Shipments.OrderID")]
        public string OrderId { get; set; }
        public int OrderNumber { get; set; }
        public string OrderCode { get; set; }
        [GrandResourceDisplayName("Admin.Orders.Shipments.TotalWeight")]
        public string TotalWeight { get; set; }
        [GrandResourceDisplayName("Admin.Orders.Shipments.TrackingNumber")]
        public string TrackingNumber { get; set; }
        public string TrackingNumberUrl { get; set; }

        [GrandResourceDisplayName("Admin.Orders.Shipments.ShippedDate")]
        public DateTime? ShippedDate { get; set; }
        public bool CanShip { get; set; }
        public DateTime? ShippedDateUtc { get; set; }

        [GrandResourceDisplayName("Admin.Orders.Shipments.DeliveryDate")]
        public DateTime? DeliveryDate { get; set; }
        public bool CanDeliver { get; set; }
        public DateTime? DeliveryDateUtc { get; set; }

        [GrandResourceDisplayName("Admin.Orders.Shipments.AdminComment")]
        public string AdminComment { get; set; }

        public List<ShipmentItemModel> Items { get; set; }

        public IList<ShipmentStatusEventModel> ShipmentStatusEvents { get; set; }

        //shipment notes
        [GrandResourceDisplayName("Admin.Orders.Shipments.ShipmentNotes.Fields.DisplayToCustomer")]
        public bool AddShipmentNoteDisplayToCustomer { get; set; }
        [GrandResourceDisplayName("Admin.Orders.Shipments.ShipmentNotes.Fields.Note")]

        public string AddShipmentNoteMessage { get; set; }
        public bool AddShipmentNoteHasDownload { get; set; }
        [GrandResourceDisplayName("Admin.Orders.Shipments.ShipmentNotes.Fields.Download")]
        [UIHint("Download")]
        public string AddShipmentNoteDownloadId { get; set; }


        #region Nested classes

        public partial class ShipmentItemModel : BaseGrandEntityModel
        {
            public ShipmentItemModel()
            {
                AvailableWarehouses = new List<WarehouseInfo>();
            }

            public string OrderItemId { get; set; }
            public string ProductId { get; set; }
            [GrandResourceDisplayName("Admin.Orders.Shipments.Products.ProductName")]
            public string ProductName { get; set; }
            public string Sku { get; set; }
            public string AttributeInfo { get; set; }
            public string RentalInfo { get; set; }
            public bool ShipSeparately { get; set; }

            //weight of one item (product)
            [GrandResourceDisplayName("Admin.Orders.Shipments.Products.ItemWeight")]
            public string ItemWeight { get; set; }
            [GrandResourceDisplayName("Admin.Orders.Shipments.Products.ItemDimensions")]
            public string ItemDimensions { get; set; }

            public int QuantityToAdd { get; set; }
            public int QuantityOrdered { get; set; }
            [GrandResourceDisplayName("Admin.Orders.Shipments.Products.QtyShipped")]
            public int QuantityInThisShipment { get; set; }
            public int QuantityInAllShipments { get; set; }

            public string ShippedFromWarehouse { get; set; }
            public bool AllowToChooseWarehouse { get; set; }
            //used before a shipment is created
            public List<WarehouseInfo> AvailableWarehouses { get; set; }
            public string WarehouseId { get; set; }

            #region Nested Classes
            public class WarehouseInfo : BaseGrandModel
            {
                public string WarehouseId { get; set; }
                public string WarehouseName { get; set; }
                public int StockQuantity { get; set; }
                public int ReservedQuantity { get; set; }
                public int PlannedQuantity { get; set; }
            }

            #endregion
        }

        public partial class ShipmentNote : BaseGrandEntityModel
        {
            public string ShipmentId { get; set; }
            [GrandResourceDisplayName("Admin.Orders.Shipments.ShipmentNotes.Fields.DisplayToCustomer")]
            public bool DisplayToCustomer { get; set; }
            [GrandResourceDisplayName("Admin.Orders.Shipments.ShipmentNotes.Fields.Note")]
            public string Note { get; set; }
            [GrandResourceDisplayName("Admin.Orders.Shipments.ShipmentNotes.Fields.Download")]
            public string DownloadId { get; set; }
            [GrandResourceDisplayName("Admin.Orders.Shipments.ShipmentNotes.Fields.Download")]
            public Guid DownloadGuid { get; set; }
            [GrandResourceDisplayName("Admin.Orders.Shipments.ShipmentNotes.Fields.CreatedOn")]
            public DateTime CreatedOn { get; set; }
            [GrandResourceDisplayName("Admin.Orders.Shipments.ShipmentNotes.Fields.CreatedByCustomer")]
            public bool CreatedByCustomer { get; set; }
        }

        public partial class ShipmentStatusEventModel : BaseGrandModel
        {
            public string EventName { get; set; }
            public string Location { get; set; }
            public string Country { get; set; }
            public DateTime? Date { get; set; }
        }

        #endregion
    }
}