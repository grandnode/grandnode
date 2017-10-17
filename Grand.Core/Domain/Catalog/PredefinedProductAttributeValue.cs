using Grand.Core.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a predefined (default) product attribute value
    /// </summary>
    public partial class PredefinedProductAttributeValue : SubBaseEntity, ILocalizedEntity
    {
        public PredefinedProductAttributeValue()
        {
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the product attribute identifier
        /// </summary>
        public string ProductAttributeId { get; set; }

        /// <summary>
        /// Gets or sets the product attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the price adjustment
        /// </summary>
        public decimal PriceAdjustment { get; set; }

        /// <summary>
        /// Gets or sets the weight adjustment
        /// </summary>
        public decimal WeightAdjustment { get; set; }

        /// <summary>
        /// Gets or sets the attibute value cost
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value is pre-selected
        /// </summary>
        public bool IsPreSelected { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }
    }
}
