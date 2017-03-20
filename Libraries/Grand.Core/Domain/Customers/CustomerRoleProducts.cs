using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer role
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class CustomerRoleProduct : BaseEntity
    {

        /// <summary>
        /// Gets or sets the customer role id
        /// </summary>
        public string CustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the product Id
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

    }

}