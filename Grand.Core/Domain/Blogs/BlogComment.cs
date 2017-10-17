using System;

namespace Grand.Core.Domain.Blogs
{
    /// <summary>
    /// Represents a blog comment
    /// </summary>
    public partial class BlogComment : BaseEntity
    {

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the comment text
        /// </summary>
        public string CommentText { get; set; }

        /// <summary>
        /// Gets or sets the blog post title
        /// </summary>
        public string BlogPostTitle { get; set; }

        /// <summary>
        /// Gets or sets the blog post identifier
        /// </summary>
        public string BlogPostId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

       
    }
}