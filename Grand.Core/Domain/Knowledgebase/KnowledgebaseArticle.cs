using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Core.Domain.Knowledgebase
{
    public class KnowledgebaseArticle : BaseEntity, ITreeNode
    {
        /// <summary>
        /// Gets or sets title of the article
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets content of the article
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets parent category Id
        /// </summary>
        public string ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets display order
        /// </summary>
        public int DisplayOrder { get; set; }

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
