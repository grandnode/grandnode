using MongoDB.Bson.Serialization.Attributes;
using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;

namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents a campaign
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class BannerActive : BaseEntity
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public int CustomerId { get; set; }
        public int CustomerActionId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    [BsonIgnoreExtraElements]
    public partial class BannerArchive : BaseEntity
    {
        public int BannerActiveId { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public int CustomerId { get; set; }
        public int CustomerActionId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime BACreatedOnUtc { get; set; }
    }

}
