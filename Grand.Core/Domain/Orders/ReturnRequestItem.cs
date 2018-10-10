namespace Grand.Core.Domain.Orders
{
    public class ReturnRequestItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
        public string OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the reason for return
        /// </summary>
        public string ReasonForReturn { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the requested action
        /// </summary>
        public string RequestedAction { get; set; }
    }
}
