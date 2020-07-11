using Grand.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Domain.Common
{
    /// <summary>
    /// Represents an address attribute value
    /// </summary>
    public partial class AddressAttributeValue : BaseEntity, ILocalizedEntity
    {
        public AddressAttributeValue()
        {
            Locales = new List<LocalizedProperty>();
        }

        /// <summary>
        /// Gets or sets the address attribute identifier
        /// </summary>
        public string AddressAttributeId { get; set; }

        /// <summary>
        /// Gets or sets the checkout attribute name
        /// </summary>
        public string Name { get; set; }

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
