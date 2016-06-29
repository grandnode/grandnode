using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents a campaign
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class Campaign : BaseEntity
    {
        private ICollection<string> _customerTags;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the store identifier  which subscribers it will be sent to; set 0 for all newsletter subscribers
        /// </summary>
        public string StoreId { get; set; }

        public DateTime? CustomerCreatedDateFrom { get; set; }
        public DateTime? CustomerCreatedDateTo { get; set; }
        public DateTime? CustomerLastActivityDateFrom { get; set; }
        public DateTime? CustomerLastActivityDateTo { get; set; }
        public DateTime? CustomerLastPurchaseDateFrom { get; set; }
        public DateTime? CustomerLastPurchaseDateTo { get; set; }

        public int CustomerHasOrders { get; set; }
        [BsonIgnoreAttribute]
        public CampaignCondition CustomerHasOrdersCondition {
            get { return (CampaignCondition)CustomerHasOrders; }
            set { this.CustomerHasOrders = (int)value; }
        }

        public int CustomerHasShoppingCart { get; set; }
        [BsonIgnoreAttribute]
        public CampaignCondition CustomerHasShoppingCartCondition {
            get { return (CampaignCondition)CustomerHasShoppingCart; }
            set { this.CustomerHasShoppingCart = (int)value; }
        }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the customer tags
        /// </summary>
        public virtual ICollection<string> CustomerTags
        {
            get { return _customerTags ?? (_customerTags = new List<string>()); }
            protected set { _customerTags = value; }
        }

    }
}
