using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Grand.Core.Domain.Catalog
{
    [BsonIgnoreExtraElements]
    public partial class ProductDeleted: Product
    {
        public DateTime DeletedOnUtc { get; set; }
    }
}
