using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;

namespace Nop.Core
{
    
    [BsonIgnoreExtraElements]
    public abstract partial class SubBaseEntity: ParentEntity
    {
        
    }
}
