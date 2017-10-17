using System;

namespace Grand.Core.Domain.Logging
{
    /// <summary>
    /// Represents an activity log record
    /// </summary>
    public partial class ActivityLog : BaseEntity
    {
        /// <summary>
        /// Gets or sets the activity log type identifier
        /// </summary>
        public string ActivityLogTypeId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the entity key identifier
        /// </summary>
        public string EntityKeyId { get; set; }
        /// <summary>
        /// Gets or sets the activity comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


    }

    /// <summary>
    /// Represents an activity stats record - Auxiliary class to reports
    /// </summary>
    public class ActivityStats
    {
        public string ActivityLogTypeId { get; set; }
        public string EntityKeyId { get; set; }
        public int Count { get; set; }
    }

}
