
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product also purchased
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class ProductAlsoPurchased : BaseEntity
    {
        public string ProductId { get; set; }
        public string ProductId2 { get; set; }
        public string OrderId { get; set; }
        public DateTime CreatedOrderOnUtc { get; set; }
        public int Quantity { get; set; }
        public string StoreId { get; set; }
    }

}
