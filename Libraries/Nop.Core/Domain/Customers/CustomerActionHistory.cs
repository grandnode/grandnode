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
        public int CustomerActionId { get; set; }
        public int CustomerId { get; set; }

    }
}
