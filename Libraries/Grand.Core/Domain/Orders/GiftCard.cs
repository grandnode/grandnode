using System;
using System.Collections.Generic;
using Grand.Core.Domain.Catalog;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Grand.Core.Domain.Orders
{
    /// <summary>
    /// Represents a gift card
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class GiftCard : BaseEntity
    {
        private ICollection<GiftCardUsageHistory> _giftCardUsageHistory;
        
        /// <summary>
        /// Gets or sets the associated order item identifier
        /// </summary>
        public string PurchasedWithOrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the gift card type identifier
        /// </summary>
        public int GiftCardTypeId { get; set; }

        /// <summary>
        /// Gets or sets the amount
        /// </summary>
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gift card is activated
        /// </summary>
        public bool IsGiftCardActivated { get; set; }

        /// <summary>
        /// Gets or sets a gift card coupon code
        /// </summary>
        public string GiftCardCouponCode { get; set; }

        /// <summary>
        /// Gets or sets a recipient name
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Gets or sets a recipient email
        /// </summary>
        public string RecipientEmail { get; set; }

        /// <summary>
        /// Gets or sets a sender name
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets a sender email
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Gets or sets a message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether recipient is notified
        /// </summary>
        public bool IsRecipientNotified { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the gift card type
        /// </summary>
        [BsonIgnoreAttribute]
        public GiftCardType GiftCardType
        {
            get
            {
                return (GiftCardType)this.GiftCardTypeId;
            }
            set
            {
                this.GiftCardTypeId = (int)value;
            }
        }
        
        /// <summary>
        /// Gets or sets the gift card usage history
        /// </summary>
        public virtual ICollection<GiftCardUsageHistory> GiftCardUsageHistory
        {
            get { return _giftCardUsageHistory ?? (_giftCardUsageHistory = new List<GiftCardUsageHistory>()); }
            protected set { _giftCardUsageHistory = value; }
        }
        
        /// <summary>
        /// Gets or sets the associated order item
        /// </summary>
        public virtual OrderItem PurchasedWithOrderItem { get; set; }
    }
}
