using System;
using System.Collections.Generic;

namespace Grand.Domain.Shipping
{
    /// <summary>
    /// Represents a shipment
    /// </summary>
    public partial class Shipment : BaseEntity
    {
        private ICollection<ShipmentItem> _shipmentItems;

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the vendor identifier
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// Gets or sets the tracking number of this shipment
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the total weight of this shipment
        /// </summary>
        public decimal? TotalWeight { get; set; }

        /// <summary>
        /// Gets or sets the shipped date and time
        /// </summary>
        public DateTime? ShippedDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the delivery date and time
        /// </summary>
        public DateTime? DeliveryDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets the entity creation date
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the shipment number
        /// </summary>
        public virtual int ShipmentNumber { get; set; }

        /// <summary>
        /// Gets or sets the shipment items
        /// </summary>
        public virtual ICollection<ShipmentItem> ShipmentItems
        {
            get { return _shipmentItems ?? (_shipmentItems = new List<ShipmentItem>()); }
            protected set { _shipmentItems = value; }
        }
    }
}