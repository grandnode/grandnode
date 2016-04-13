using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Messages
{
    [BsonIgnoreExtraElements]
    public partial class BannerArchive : BaseEntity
    {
        public string BannerActiveId { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public string CustomerId { get; set; }
        public string CustomerActionId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime BACreatedOnUtc { get; set; }
    }
}
