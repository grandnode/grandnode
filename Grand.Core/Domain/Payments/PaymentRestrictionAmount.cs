namespace Grand.Core.Domain.Payments
{
    /// <summary>
    /// Represents a payment status enumeration
    /// </summary>
    public enum PaymentRestrictionAmount
    {
        /// <summary>
        /// Restriction by order total
        /// </summary>
        OrderTotal = 0,
        /// <summary>
        /// Restriction by order sub-total
        /// </summary>
        SubTotal = 1,
    }
}
