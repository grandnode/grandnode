using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Grand.Core.Domain.Common
{
    [BsonIgnoreExtraElements]
    public partial class GenericAttributeBaseEntity : BaseEntity
    {
    }
}
