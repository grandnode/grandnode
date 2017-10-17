using System;
using System.Collections.Generic;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Seo;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Security;

namespace Grand.Core.Domain.News
{
    /// <summary>
    /// Represents a news item
    /// </summary>
    public partial class NewsItem : BaseEntity, ISlugSupported, IStoreMappingSupported, ILocalizedEntity, IAclSupported
    {
        private ICollection<NewsComment> _newsComments;

        public NewsItem()
        {
            Stores = new List<string>();
            Locales = new List<LocalizedProperty>();
            CustomerRoles = new List<string>();
        }
        
        /// <summary>
        /// Gets or sets the news title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
        public string PictureId { get; set; }

        /// <summary>
        /// Gets or sets the sename
        /// </summary>
        public string SeName { get; set; }

        /// <summary>
        /// Gets or sets the short text
        /// </summary>
        public string Short { get; set; }

        /// <summary>
        /// Gets or sets the full text
        /// </summary>
        public string Full { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the news item is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the news item start date and time
        /// </summary>
        public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the news item end date and time
        /// </summary>
        public DateTime? EndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the news post comments are allowed 
        /// </summary>
        public bool AllowComments { get; set; }

        /// <summary>
        /// Gets or sets the total number of comments
        /// <remarks>
        /// We use this property for performance optimization (no SQL command executed)
        /// </remarks>
        /// </summary>
        public int CommentCount { get; set; }

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
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
        public bool SubjectToAcl { get; set; }
        public IList<string> CustomerRoles { get; set; }

        /// <summary>
        /// Gets or sets the news comments
        /// </summary>
        public virtual ICollection<NewsComment> NewsComments
        {
            get { return _newsComments ?? (_newsComments = new List<NewsComment>()); }
            protected set { _newsComments = value; }
        }
        
    }
}