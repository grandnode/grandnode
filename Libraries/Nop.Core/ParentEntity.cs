using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;

namespace Nop.Core
{
    public abstract class ParentEntity
    {
        public ParentEntity()
        {
            Id = ObjectId.GenerateNewId().ToString();
            
        }

        public string Id { get; set; }

    }
}
