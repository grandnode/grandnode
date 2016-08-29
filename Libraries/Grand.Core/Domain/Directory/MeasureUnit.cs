using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Directory
{
    /// <summary>
    /// Represents a measure weight
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class MeasureUnit : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
