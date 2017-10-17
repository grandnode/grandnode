using System;

namespace Grand.Core.Domain.Vendors
{
    /// <summary>
    /// Represents a vendor note
    /// </summary>
    public partial class VendorNote : SubBaseEntity
    {

        /// <summary>
        /// Gets or sets the vendor identifier
        /// </summary>
        public string VendorId { get; set; }
        /// <summary>
        /// Gets or sets the note
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the date and time of vendor note creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }
}
