using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Grand.Core.Domain.Common;
using System;
using System.Collections.Generic;

namespace Grand.Core
{
    
    [BsonIgnoreExtraElements]
    public abstract partial class SubBaseEntity: ParentEntity
    {
        
    }
}
