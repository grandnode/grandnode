using System;

namespace Grand.Core.Domain.Orders
{
    /// <summary>
    /// Represents a recurring payment history
    /// </summary>
    public partial class RecurringPaymentHistory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the recurring payment identifier
        /// </summary>
        public string RecurringPaymentId { get; set; }

        /// <summary>
        /// Gets or sets the recurring payment identifier
        /// </summary>
        public string OrderId { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        
        /// <summary>
        /// Gets the recurring payment
        /// </summary>
        public virtual RecurringPayment RecurringPayment { get; set; }

    }
}
