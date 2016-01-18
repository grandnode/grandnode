using System;
using Nop.Core.Domain.Customers;
using MongoDB.Bson.Serialization.Attributes;

namespace Nop.Core.Domain.News
{
    /// <summary>
    /// Represents a news comment
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class NewsComment : BaseEntity
    {
        /// <summary>
        /// Gets or sets the comment title
        /// </summary>
        public string CommentTitle { get; set; }

        /// <summary>
        /// Gets or sets the comment text
        /// </summary>
        public string CommentText { get; set; }

        /// <summary>
        /// Gets or sets the news item identifier
        /// </summary>
        public int NewsItemId { get; set; }
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }
}