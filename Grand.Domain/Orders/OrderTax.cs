using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Domain.Orders
{
    public class OrderTax : SubBaseEntity
    {
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal Percent { get; set; }

        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public decimal Amount { get; set; }
    }
}
