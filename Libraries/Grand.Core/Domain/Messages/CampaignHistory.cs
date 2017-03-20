using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Grand.Core.Domain.Messages
{
    /// <summary>
    /// Represents a campaign history
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class CampaignHistory : BaseEntity
    {
        
        public string CampaignId { get; set; }

        public string Email { get; set; }

        public string CustomerId { get; set; }

        public string StoreId { get; set; }

        public DateTime CreatedDateUtc { get; set; }

    }
}
