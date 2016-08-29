﻿using System;
using Grand.Core.Domain.Customers;
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Forums
{
    /// <summary>
    /// Represents a private message
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class PrivateMessage : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier who sent the message
        /// </summary>
        public string FromCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier who should receive the message
        /// </summary>
        public string ToCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indivating whether message is read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets a value indivating whether message is deleted by author
        /// </summary>
        public bool IsDeletedByAuthor { get; set; }

        /// <summary>
        /// Gets or sets a value indivating whether message is deleted by recipient
        /// </summary>
        public bool IsDeletedByRecipient { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }
}
