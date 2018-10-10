namespace Grand.Core.Domain.Orders
{
    /// <summary>
    /// Represents a return status
    /// </summary>
    public enum ReturnRequestStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 0,
        /// <summary>
        /// Accepted
        /// </summary>
        Accepted = 10,
        /// <summary>
        /// Rejected
        /// </summary>
        Rejected = 20,
        /// <summary>
        /// Cancelled
        /// </summary>
        Cancelled = 30,
        /// <summary>
        /// Completed
        /// </summary>
        Completed = 40
    }
}
