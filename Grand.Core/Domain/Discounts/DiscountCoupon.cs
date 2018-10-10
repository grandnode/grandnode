namespace Grand.Core.Domain.Discounts
{
    public class DiscountCoupon : BaseEntity
    {
        /// <summary>
        /// Gets or sets the coupon code
        /// </summary>
        public string CouponCode { get; set; }

        /// <summary>
        /// Gets or sets the discount id
        /// </summary>
        public string DiscountId { get; set; }

        /// <summary>
        /// Gets or sets discount used
        /// </summary>
        public bool Used { get; set; }

        /// <summary>
        /// How many times was used
        /// </summary>
        public int Qty { get; set; }
    }
}
