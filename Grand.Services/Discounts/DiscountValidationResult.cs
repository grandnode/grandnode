namespace Grand.Services.Discounts
{
    /// <summary>
    /// Represents a result of discount validation
    /// </summary>
    public partial class DiscountValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether discount is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets an error that a customer should see when enterting a coupon code (in case if "IsValid" is set to "false")
        /// </summary>
        public string UserError { get; set; }

        /// <summary>
        /// Gets or sets a coupon code value
        /// </summary>
        public string CouponCode { get; set; }
    }
}
