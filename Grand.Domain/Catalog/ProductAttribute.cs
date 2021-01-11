using Grand.Domain.Localization;
using Grand.Domain.Stores;
using System.Collections.Generic;

namespace Grand.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute
    /// </summary>
    public partial class ProductAttribute : BaseEntity, IStoreMappingSupported, ILocalizedEntity
    {
        public ProductAttribute()
        {
            Locales = new List<LocalizedProperty>();
            Stores = new List<string>();
            PredefinedProductAttributeValues = new List<PredefinedProductAttributeValue>();
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
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }
        public IList<PredefinedProductAttributeValue> PredefinedProductAttributeValues { get; set; }
    }
}
