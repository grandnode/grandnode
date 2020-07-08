using Grand.Domain.Localization;
using Grand.Domain.Security;
using Grand.Domain.Seo;
using Grand.Domain.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Domain.Knowledgebase
{
    public class KnowledgebaseArticle : BaseEntity, ITreeNode, ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported
    {
        public KnowledgebaseArticle()
        {
            CustomerRoles = new List<string>();
            Locales = new List<LocalizedProperty>();
            RelatedArticles = new List<string>();
            Stores = new List<string>();
        }

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
        /// Gets or sets published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
        public bool SubjectToAcl { get; set; }
        public IList<string> CustomerRoles { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string SeName { get; set; }

        /// <summary>
        /// Gets or sets meta keywords
        /// </summary>
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets meta description
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets meta title
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether to show article on knowledgebase homepage
        /// </summary>
        public bool ShowOnHomepage { get; set; }

        /// <summary>
        /// Gets or sets list of related articles ids
        /// </summary>
        public IList<string> RelatedArticles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the comments are allowed 
        /// </summary>
        public bool AllowComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }
    }
}
