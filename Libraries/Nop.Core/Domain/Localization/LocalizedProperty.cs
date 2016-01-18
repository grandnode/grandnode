using MongoDB.Bson.Serialization.Attributes;

namespace Nop.Core.Domain.Localization
{
    /// <summary>
    /// Represents a localized property
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class LocalizedProperty : BaseEntity
    {
        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the locale key
        /// </summary>
        public string LocaleKey { get; set; }

        /// <summary>
        /// Gets or sets the locale value
        /// </summary>
        public string LocaleValue { get; set; }
        
    }
}
