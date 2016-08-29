using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Core.Domain.Common
{
    [BsonIgnoreExtraElements]
    public partial class GrandNodeVersion: BaseEntity
    {
        public string DataBaseVersion { get; set; }
    }
}
