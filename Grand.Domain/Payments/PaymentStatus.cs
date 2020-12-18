namespace Grand.Domain.Payments
{
    /// <summary>
    /// Represents a payment status enumeration
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 10,
        /// <summary>
        /// Authorized
        /// </summary>
        Authorized = 20,
        /// <summary>
        /// Paid
        /// </summary>
        Paid = 30,
        /// <summary>
        /// Pending Partially Refunded
        /// </summary>
        PendingPartiallyRefunded = 34,
        /// <summary>
        /// Partially Refunded
        /// </summary>
        PartiallyRefunded = 35,
        /// <summary>
        /// Pending Refunded
        /// </summary>
        PendingRefunded = 39,
        /// <summary>
        /// Refunded
        /// </summary>
        Refunded = 40,
        /// <summary>
        /// Voided
        /// </summary>
        Voided = 50,
    }
}
