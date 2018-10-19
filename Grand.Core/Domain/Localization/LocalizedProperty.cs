namespace Grand.Core.Domain.Localization
{
    /// <summary>
    /// Represents a localized property
    /// </summary>
    public partial class LocalizedProperty : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public string LanguageId { get; set; }

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
