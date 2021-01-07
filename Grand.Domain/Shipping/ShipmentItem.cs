using Grand.Domain.Common;
using System;
using System.Collections.Generic;

namespace Grand.Domain.Shipping
{
    /// <summary>
    /// Represents a shipment item
    /// </summary>
    public partial class ShipmentItem : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the shipment identifier
        /// </summary>
        public string ShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
        public string OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the product
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product attribute xml
        /// </summary>
        [Obsolete("Will be removed in version 5.0.0 - this field was replaced by Attributes")] 
        public string AttributeXML { get; set; }
        public IList<CustomAttribute> Attributes { get; set; } = new List<CustomAttribute>();

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the warehouse identifier
        /// </summary>
        public string WarehouseId { get; set; }
    }
}