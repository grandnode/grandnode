using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Customers
{
    /// <summary>
    /// Represents a product tag
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class CustomerTag : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }
    }
}
