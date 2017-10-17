using System;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a tier price
    /// </summary>
    public partial class TierPrice : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the store identifier (0 - all stores)
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the customer role identifier
        /// </summary>
        public string CustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the price
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Gets or sets the start date and time of the special price
        /// </summary>
        public DateTime? StartDateTimeUtc { get; set; }
        /// <summary>
        /// Gets or sets the end date and time of the special price
        /// </summary>
        public DateTime? EndDateTimeUtc { get; set; }

    }
}
