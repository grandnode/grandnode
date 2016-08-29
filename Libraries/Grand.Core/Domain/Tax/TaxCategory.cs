
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Tax
{
    /// <summary>
    /// Represents a tax category
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class TaxCategory : BaseEntity
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
