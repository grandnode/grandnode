using Grand.Core.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute
    /// </summary>
    public partial class ProductAttribute : BaseEntity, ILocalizedEntity
    {
        public ProductAttribute()
        {
            Locales = new List<LocalizedProperty>();
            PredefinedProductAttributeValues = new List<PredefinedProductAttributeValue>();
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
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }
        public IList<PredefinedProductAttributeValue> PredefinedProductAttributeValues { get; set; }
    }
}
