using System;
using Nop.Core.Domain.Customers;
using MongoDB.Bson.Serialization.Attributes;

namespace Nop.Core.Domain.Customers
{

    [BsonIgnoreExtraElements]
    public partial class CustomerActionConditionType : BaseEntity
    {
        public string Name { get; set; }

    }   
     

}
