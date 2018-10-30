using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Seo;
using Grand.Core.Domain.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a manufacturer
    /// </summary>
    public partial class Manufacturer : BaseEntity, ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported
    {
        private ICollection<string> _appliedDiscounts;

        public Manufacturer()
        {
            CustomerRoles = new List<string>();
            Stores = new List<string>();
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sename
        /// </summary>
        public string SeName { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value of used manufacturer template identifier
        /// </summary>
        public string ManufacturerTemplateId { get; set; }

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
        /// Gets or sets the parent picture identifier
        /// </summary>
        public string PictureId { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select the page size
        /// </summary>
        public bool AllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options
        /// </summary>
        public string PageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets the available price ranges
        /// </summary>
        public string PriceRanges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the manufacturer on home page
        /// </summary>
        public bool ShowOnHomePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show featured products on home page
        /// </summary>
        public bool FeaturedProductsOnHomaPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include this manufacturer in the top menu
        /// </summary>
        public bool IncludeInTopMenu { get; set; }

        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
 	    /// </summary>
        public bool SubjectToAcl { get; set; }
        public IList<string> CustomerRoles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
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

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }

        /// <summary>
        /// Gets or sets the collection of applied discounts
        /// </summary>
        public virtual ICollection<string> AppliedDiscounts
        {
            get { return _appliedDiscounts ?? (_appliedDiscounts = new List<string>()); }
            protected set { _appliedDiscounts = value; }
        }
    }
}
