namespace Grand.Domain.Common
{
    /// <summary>
    /// Represents a generic attribute
    /// </summary>
    public partial class GenericAttribute
    {
        /// <summary>
        /// Gets or sets the key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public string StoreId { get; set; }
        
    }
}
