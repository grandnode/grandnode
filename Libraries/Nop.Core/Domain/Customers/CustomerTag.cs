using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using MongoDB.Bson.Serialization.Attributes;

namespace Nop.Core.Domain.Customers
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
