using Grand.Core.Domain.Common;
using System;
using System.Collections.Generic;

namespace Grand.Core.Domain.Orders
{
    /// <summary>
    /// Represents a return request
    /// </summary>
    public partial class ReturnRequest : BaseEntity
    {
        public ReturnRequest()
        {
            ReturnRequestItems = new List<ReturnRequestItem>();
        }

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
        /// Gets or sets the return request items
        /// </summary>
        public IList<ReturnRequestItem> ReturnRequestItems { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

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

        /// <summary>
        /// Gets or sets the pickup date
        /// </summary>
        public DateTime PickupDate { get; set; }

        /// <summary>
        /// Gets or sets the pickup address
        /// </summary>
        public Address PickupAddress { get; set; }

        /// <summary>
        /// Get or sets notify customer
        /// </summary>
        public bool NotifyCustomer { get; set; }
    }
}
