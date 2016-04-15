using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer action
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class CustomerActionHistory : BaseEntity
    {
        public string CustomerActionId { get; set; }
        public string CustomerId { get; set; }
        public DateTime CreateDateUtc { get; set; }

    }
}
