using System;

namespace Grand.Domain.Knowledgebase
{
    public partial class KnowledgebaseArticleComment : BaseEntity
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
        public string ArticleTitle { get; set; }

        /// <summary>
        /// Gets or sets the blog post identifier
        /// </summary>
        public string ArticleId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}
