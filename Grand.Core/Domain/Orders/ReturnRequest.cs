using System;

namespace Grand.Core.Domain.Orders
{
    /// <summary>
    /// Represents a return request
    /// </summary>
    public partial class ReturnRequest : BaseEntity
    {
        public int ReturnNumber { get; set; }
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
        public string OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the reason to return
        /// </summary>
        public string ReasonForReturn { get; set; }

        /// <summary>
        /// Gets or sets the requested action
        /// </summary>
        public string RequestedAction { get; set; }

        /// <summary>
        /// Gets or sets the customer comments
        /// </summary>
        public string CustomerComments { get; set; }

        /// <summary>
        /// Gets or sets the staff notes
        /// </summary>
        public string StaffNotes { get; set; }

        /// <summary>
        /// Gets or sets the return status identifier
        /// </summary>
        public int ReturnRequestStatusId { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }
        
        /// <summary>
        /// Gets or sets the return status
        /// </summary>
        public ReturnRequestStatus ReturnRequestStatus
        {
            get
            {
                return (ReturnRequestStatus)this.ReturnRequestStatusId;
            }
            set
            {
                this.ReturnRequestStatusId = (int)value;
            }
        }
        
    }
}
