using System;
using System.Collections.Generic;
using Grand.Core.Domain.Customers;
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product review
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class ProductReview : BaseEntity
    {
        private ICollection<ProductReviewHelpfulness> _productReviewHelpfulnessEntries;

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public string StoreId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the content is approved
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the review text
        /// </summary>
        public string ReviewText { get; set; }

        /// <summary>
        /// Review rating
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Review helpful votes total
        /// </summary>
        public int HelpfulYesTotal { get; set; }

        /// <summary>
        /// Review not helpful votes total
        /// </summary>
        public int HelpfulNoTotal { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the entries of product review helpfulness
        /// </summary>
        public virtual ICollection<ProductReviewHelpfulness> ProductReviewHelpfulnessEntries
        {
            get { return _productReviewHelpfulnessEntries ?? (_productReviewHelpfulnessEntries = new List<ProductReviewHelpfulness>()); }
            protected set { _productReviewHelpfulnessEntries = value; }
        }
    }
}
