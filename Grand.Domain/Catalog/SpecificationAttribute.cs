using System.Collections.Generic;
using Grand.Domain.Localization;
using Grand.Domain.Stores;

namespace Grand.Domain.Catalog
{
    /// <summary>
    /// Represents a specification attribute
    /// </summary>
    public partial class SpecificationAttribute : BaseEntity, IStoreMappingSupported, ILocalizedEntity
    {
        private ICollection<SpecificationAttributeOption> _specificationAttributeOptions;

        public SpecificationAttribute()
        {
            Locales = new List<LocalizedProperty>();
            Stores = new List<string>();
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
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

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
        /// Gets or sets the specification attribute options
        /// </summary>
        public virtual ICollection<SpecificationAttributeOption> SpecificationAttributeOptions
        {
            get { return _specificationAttributeOptions ??= new List<SpecificationAttributeOption>(); }
            protected set { _specificationAttributeOptions = value; }
        }
    }
}
