﻿using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Grand.Core.Domain.Messages
{
    /// <summary>
    /// Represents NewsLetterSubscription entity
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class NewsLetterSubscription : BaseEntity
    {       
        /// <summary>
        /// Gets or sets the newsletter subscription GUID
        /// </summary>
        public Guid NewsLetterSubscriptionGuid { get; set; }

        /// <summary>
        /// Gets or sets the Customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the subcriber email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subscription is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the store identifier in which a customer has subscribed to newsletter
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when subscription was created
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}
