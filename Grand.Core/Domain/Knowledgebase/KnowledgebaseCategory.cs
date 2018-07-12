using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Seo;
using Grand.Core.Domain.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Core.Domain.Knowledgebase
{
    public class KnowledgebaseCategory : BaseEntity, ITreeNode, ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported
    {
        public KnowledgebaseCategory()
        {
            CustomerRoles = new List<string>();
            Locales = new List<LocalizedProperty>();
            Stores = new List<string>();
        }

        /// <summary>
        /// Gets or sets name of the category
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets parent category Id
        /// </summary>
        public string ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets description
        /// </summary>
        public string Description { get; set; }

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
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }
    }
}
