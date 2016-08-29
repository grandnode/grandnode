using MongoDB.Bson.Serialization.Attributes;
using Grand.Core.Domain.Localization;
using System;
using System.Collections.Generic;

namespace Grand.Core.Domain.Messages
{
    [BsonIgnoreExtraElements]
    public partial class BannerActive : BaseEntity
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public string CustomerId { get; set; }
        public string CustomerActionId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

}
