using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Grand.Domain.Orders
{
    /// <summary>
    /// Represents an order item
    /// </summary>
    public partial class OrderItem : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
        public Guid OrderItemGuid { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the vendor identifier
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// Gets or sets the warehouse identifier
        /// </summary>
        public string WarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price without discount in primary store currency (incl tax)
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal UnitPriceWithoutDiscInclTax { get; set; }

        /// <summary>
        /// Gets or sets the unit price without discount in primary store currency (excl tax)
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal UnitPriceWithoutDiscExclTax { get; set; }

        /// <summary>
        /// Gets or sets the unit price in primary store currency (incl tax)
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal UnitPriceInclTax { get; set; }

        /// <summary>
        /// Gets or sets the unit price in primary store currency (excl tax)
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal UnitPriceExclTax { get; set; }

        /// <summary>
        /// Gets or sets the price in primary store currency (incl tax)
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal PriceInclTax { get; set; }

        /// <summary>
        /// Gets or sets the price in primary store currency (excl tax)
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal PriceExclTax { get; set; }

        /// <summary>
        /// Gets or sets the discount amount (incl tax)
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal DiscountAmountInclTax { get; set; }

        /// <summary>
        /// Gets or sets the discount amount (excl tax)
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal DiscountAmountExclTax { get; set; }

        /// <summary>
        /// Gets or sets the original cost of this order item (when an order was placed), qty 1
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal OriginalProductCost { get; set; }

        /// <summary>
        /// Gets or sets the attribute description
        /// </summary>
        public string AttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the product attributes in XML format
        /// </summary>
        public string AttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the download count
        /// </summary>
        public int DownloadCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether download is activated
        /// </summary>
        public bool IsDownloadActivated { get; set; }

        /// <summary>
        /// Gets or sets a license download identifier (in case this is a downloadable product)
        /// </summary>
        public string LicenseDownloadId { get; set; }

        /// <summary>
        /// Gets or sets the total weight of one item
        /// </summary>       
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal? ItemWeight { get; set; }

        /// <summary>
        /// Gets or sets the rental product start date (null if it's not a rental product)
        /// </summary>
        public DateTime? RentalStartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the rental product end date (null if it's not a rental product)
        /// </summary>
        public DateTime? RentalEndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the vendor`s commission
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal Commission { get; set; }
    }
}