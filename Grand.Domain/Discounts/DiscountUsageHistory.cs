using System;

namespace Grand.Domain.Discounts
{
    /// <summary>
    /// Represents a discount usage history entry
    /// </summary>
    public partial class DiscountUsageHistory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the discount identifier
        /// </summary>
        public string DiscountId { get; set; }

        /// <summary>
        /// Gets or sets the discount coupon code
        /// </summary>
        public string CouponCode { get; set; }

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the canceled identifier
        /// </summary>
        public bool Canceled { get; set; }

    }
}
