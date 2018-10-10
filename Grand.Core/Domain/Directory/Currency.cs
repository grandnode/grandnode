using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Core.Domain.Directory
{
    /// <summary>
    /// Represents a currency
    /// </summary>
    public partial class Currency : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        public Currency()
        {
            Stores = new List<string>();
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the currency code
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the rate
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the display locale
        /// </summary>
        public string DisplayLocale { get; set; }

        /// <summary>
        /// Gets or sets the custom formatting
        /// </summary>
        public string CustomFormatting { get; set; }

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
        /// Gets or sets the rounding type identifier
        /// </summary>
        public int RoundingTypeId { get; set; }
        /// <summary>
        /// Gets or sets the rounding type
        /// </summary>
        public RoundingType RoundingType
        {
            get
            {
                return (RoundingType) RoundingTypeId;
            }
            set
            {
                RoundingTypeId = (int) value;
            }
        }

        /// <summary>
        /// Gets or sets the Midpoint rounding identifier
        /// </summary>
        public int MidpointRoundId { get; set; }

        public MidpointRounding MidpointRound
        {
            get
            {
                return (MidpointRounding)MidpointRoundId;
            }
            set
            {
                MidpointRoundId = (int)value;
            }
        }

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
    }

}
