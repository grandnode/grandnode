using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Common
{
    [BsonIgnoreExtraElements]
    public partial class GrandNodeVersion: BaseEntity
    {
        public string DataBaseVersion { get; set; }
    }
}
