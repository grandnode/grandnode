﻿using System;
using Grand.Core.Domain.Customers;
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Forums
{
    /// <summary>
    /// Represents a forum post
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class ForumPost : BaseEntity
    {
        /// <summary>
        /// Gets or sets the forum topic identifier
        /// </summary>
        public string TopicId { get; set; }

        /// <summary>
        /// Gets or sets the forum identifier
        /// </summary>
        public string ForumId { get; set; }

        /// <summary>
        /// Gets or sets the forum group identifier
        /// </summary>
        public string ForumGroupId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

    }
}
