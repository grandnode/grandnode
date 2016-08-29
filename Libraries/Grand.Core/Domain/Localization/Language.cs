using System.Collections.Generic;
using Grand.Core.Domain.Stores;
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Localization
{
    /// <summary>
    /// Represents a language
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class Language : BaseEntity, IStoreMappingSupported
    {
        public Language()
        {
            Stores = new List<string>();
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the language culture
        /// </summary>
        public string LanguageCulture { get; set; }

        /// <summary>
        /// Gets or sets the unique SEO code
        /// </summary>
        public string UniqueSeoCode { get; set; }
        
        /// <summary>
        /// Gets or sets the flag image file name
        /// </summary>
        public string FlagImageFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the language supports "Right-to-left"
        /// </summary>
        public bool Rtl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the default currency for this language; 0 is set when we use the default currency display order
        /// </summary>
        public string DefaultCurrencyId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the language is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
    }
}
