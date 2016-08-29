
using MongoDB.Bson.Serialization.Attributes;
using Grand.Core.Domain.Catalog;

namespace Grand.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipment item
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class ShipmentItem : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the shipment identifier
        /// </summary>
        [BsonIgnore]
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
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the warehouse identifier
        /// </summary>
        public string WarehouseId { get; set; }

    }
}