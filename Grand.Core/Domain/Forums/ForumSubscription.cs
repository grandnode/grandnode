using System;

namespace Grand.Core.Domain.Forums
{
    /// <summary>
    /// Represents a forum subscription item
    /// </summary>
    public partial class ForumSubscription : BaseEntity
    {
        /// <summary>
        /// Gets or sets the forum subscription identifier
        /// </summary>
        public Guid SubscriptionGuid { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the forum identifier
        /// </summary>
        public string ForumId { get; set; }

        /// <summary>
        /// Gets or sets the topic identifier
        /// </summary>
        public string TopicId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }
}
