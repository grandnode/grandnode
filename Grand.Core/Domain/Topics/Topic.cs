using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Seo;
using Grand.Core.Domain.Stores;
using System.Collections.Generic;

namespace Grand.Core.Domain.Topics
{
    /// <summary>
    /// Represents a topic
    /// </summary>
    public partial class Topic : BaseEntity, ILocalizedEntity, ISlugSupported, IStoreMappingSupported, IAclSupported
    {
        public Topic()
        {
            Locales = new List<LocalizedProperty>();
            Stores = new List<string>();
            CustomerRoles = new List<string>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string SystemName { get; set; }


        /// <summary>
        /// Gets or sets the sename
        /// </summary>
        public string SeName { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic should be included in sitemap
        /// </summary>
        public bool IncludeInSitemap { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic should be included in top menu
        /// </summary>
        public bool IncludeInTopMenu { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic should be included in footer (column 1)
        /// </summary>
        public bool IncludeInFooterColumn1 { get; set; }
        /// <summary>
        /// Gets or sets the value indicating whether this topic should be included in footer (column 1)
        /// </summary>
        public bool IncludeInFooterColumn2 { get; set; }
        /// <summary>
        /// Gets or sets the value indicating whether this topic should be included in footer (column 1)
        /// </summary>
        public bool IncludeInFooterColumn3 { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic is accessible when a store is closed
        /// </summary>
        public bool AccessibleWhenStoreClosed { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic is password protected
        /// </summary>
        public bool IsPasswordProtected { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value of used topic template identifier
        /// </summary>
        public string TopicTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
        public bool SubjectToAcl { get; set; }
        public IList<string> CustomerRoles { get; set; }

    }
}
