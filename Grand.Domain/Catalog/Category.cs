using Grand.Domain.Localization;
using Grand.Domain.Security;
using Grand.Domain.Seo;
using Grand.Domain.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Domain.Catalog
{
    /// <summary>
    /// Represents a category
    /// </summary>
    public partial class Category : BaseEntity, ISlugSupported, IAclSupported, ILocalizedEntity, IStoreMappingSupported, ITreeNode
    {
        private ICollection<string> _appliedDiscounts;

        public Category()
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
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        
        /// <summary>
        /// Gets or sets the bottom description
        /// </summary>
        public string BottomDescription { get; set; }

        /// <summary>
        /// Gets or sets a value of used category template identifier
        /// </summary>
        public string CategoryTemplateId { get; set; }

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
        /// Gets or sets the parent category identifier
        /// </summary>
        public string ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
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
        /// Gets or sets a value indicating whether to show the category on home page
        /// </summary>
        public bool ShowOnHomePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show featured products on home page
        /// </summary>
        public bool FeaturedProductsOnHomaPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the category on search box
        /// </summary>
        public bool ShowOnSearchBox { get; set; }

        /// <summary>
        /// Gets or sets the display order on search box category
        /// </summary>
        public int SearchBoxDisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include this category in the top menu
        /// </summary>
        public bool IncludeInTopMenu { get; set; }

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
        /// Gets or sets the name
        /// </summary>
        public string SeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the flag
        /// </summary>
        public string Flag { get; set; }

        /// <summary>
        /// Gets or sets the flag style
        /// </summary>
        public string FlagStyle { get; set; }

        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the Default sort
        /// </summary>
        public int DefaultSort { get; set; } = -1;

        /// <summary>
        /// Gets or sets the hide on catalog page (subcategories)
        /// </summary>
        public bool HideOnCatalog { get; set; }

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
