using Grand.Core.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a specification attribute option
    /// </summary>
    public partial class SpecificationAttributeOption : SubBaseEntity, ILocalizedEntity
    {
        public SpecificationAttributeOption()
        {
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the specification attribute identifier
        /// </summary>
        public string SpecificationAttributeId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        // <summary>
        /// Gets or sets the color RGB value (used when you want to display "Color squares" instead of text)
        /// </summary>
        public string ColorSquaresRgb { get; set; }

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
