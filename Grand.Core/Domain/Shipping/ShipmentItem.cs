

namespace Grand.Core.Domain.Shipping
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
        public string AttributeXML { get; set; }

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