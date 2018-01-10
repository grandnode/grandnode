using System;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product reservation
    /// </summary>
    public partial class ProductReservation : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// Gets or sets the reservation date
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Gets or sets the order id
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// Gets or sets resource name
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// Gets or sets parameter name
        /// </summary>
        public string Parameter { get; set; }
        /// <summary>
        /// Gets or sets duration
        /// </summary>
        public string Duration { get; set; }
    }
}
