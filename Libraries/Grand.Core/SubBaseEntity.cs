using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core
{
    
    [BsonIgnoreExtraElements]
    public abstract partial class SubBaseEntity: ParentEntity
    {
        
    }
}
