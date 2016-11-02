using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Common
{
    [BsonIgnoreExtraElements]
    public partial class GenericAttributeBaseEntity : BaseEntity
    {

    }
}
