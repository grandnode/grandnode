using Grand.Core.Domain.Orders;

namespace Grand.Services.Discounts
{
    /// <summary>
    /// Applied gift card
    /// </summary>
    public class AppliedDiscount
    {
        /// <summary>
        /// Gets or sets the discount Id
        /// </summary>
        public string DiscountId { get; set; }

        /// <summary>
        /// Gets or sets the discount 
        /// </summary>
        public string CouponCode { get; set; }

        public bool IsCumulative { get; set; }
    }
}
