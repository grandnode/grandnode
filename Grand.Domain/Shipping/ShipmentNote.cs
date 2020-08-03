using System;

namespace Grand.Domain.Shipping
{
    /// <summary>
    /// Represents an shipment note
    /// </summary>
    public partial class ShipmentNote : BaseEntity
    {
        /// <summary>
        /// Gets or sets the shipment identifier
        /// </summary>
        public string ShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the note
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the attached file (download) identifier
        /// </summary>
        public string DownloadId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a customer can see a note
        /// </summary>
        public bool DisplayToCustomer { get; set; }

        /// <summary>
        /// Gets or sets the date and time of shipment note creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether this shipment note was create by customer
        /// </summary>
        public bool CreatedByCustomer { get; set; }
    }
}
