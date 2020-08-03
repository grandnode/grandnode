using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Domain.Orders
{
    /// <summary>
    /// Represents a return request note
    /// </summary>
    public partial class ReturnRequestNote : BaseEntity
    {
        /// <summary>
        /// Gets or sets the return request identifier
        /// </summary>
        public string ReturnRequestId { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }

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
        /// Gets or sets the date and time of return request note creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether this return request note was create by customer
        /// </summary>
        public bool CreatedByCustomer { get; set; }
    }
}
