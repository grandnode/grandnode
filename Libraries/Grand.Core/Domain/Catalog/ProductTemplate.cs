
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product template
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class ProductTemplate : BaseEntity
    {
        /// <summary>
        /// Gets or sets the template name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the view path
        /// </summary>
        public string ViewPath { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
