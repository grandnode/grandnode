using Grand.Core.Domain.Catalog;
using System;
using System.Collections.Generic;

namespace Grand.Core.Domain.Orders
{
    /// <summary>
    /// Represents a recurring payment
    /// </summary>
    public partial class RecurringPayment : BaseEntity
    {
        private ICollection<RecurringPaymentHistory> _recurringPaymentHistory;

        /// <summary>
        /// Gets or sets the cycle length
        /// </summary>
        public int CycleLength { get; set; }

        /// <summary>
        /// Gets or sets the cycle period identifier
        /// </summary>
        public int CyclePeriodId { get; set; }

        /// <summary>
        /// Gets or sets the total cycles
        /// </summary>
        public int TotalCycles { get; set; }

        /// <summary>
        /// Gets or sets the start date
        /// </summary>
        public DateTime StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the payment is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the initial order identifier
        /// </summary>
        public int InitialOrderId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of payment creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        
        /// <summary>
        /// Gets the next payment date
        /// </summary>
        public DateTime? NextPaymentDate
        {
            get
            {
                if (!this.IsActive)
                    return null;

                var historyCollection = this.RecurringPaymentHistory;
                if (historyCollection.Count >= this.TotalCycles)
                {
                    return null;
                }

                //result
                DateTime? result = null;
               
                if (historyCollection.Count > 0)
                {
                    switch (this.CyclePeriod)
                    {
                        case RecurringProductCyclePeriod.Days:
                            result = this.StartDateUtc.AddDays((double)this.CycleLength * historyCollection.Count);
                            break;
                        case RecurringProductCyclePeriod.Weeks:
                            result = this.StartDateUtc.AddDays((double)(7 * this.CycleLength) * historyCollection.Count);
                            break;
                        case RecurringProductCyclePeriod.Months:
                            result = this.StartDateUtc.AddMonths(this.CycleLength * historyCollection.Count);
                            break;
                        case RecurringProductCyclePeriod.Years:
                            result = this.StartDateUtc.AddYears(this.CycleLength * historyCollection.Count);
                            break;
                        default:
                            throw new GrandException("Not supported cycle period");
                    }
                }
                else
                {
                    if (this.TotalCycles > 0)
                        result = this.StartDateUtc;
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the cycles remaining
        /// </summary>
        public int CyclesRemaining
        {
            get
            {
                //result
                var historyCollection = this.RecurringPaymentHistory;
                int result = this.TotalCycles - historyCollection.Count;
                if (result < 0)
                    result = 0;

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public RecurringProductCyclePeriod CyclePeriod
        {
            get
            {
                return (RecurringProductCyclePeriod)this.CyclePeriodId;
            }
            set
            {
                this.CyclePeriodId = (int)value;
            }
        }
        



        /// <summary>
        /// Gets or sets the recurring payment history
        /// </summary>
        public virtual ICollection<RecurringPaymentHistory> RecurringPaymentHistory
        {
            get { return _recurringPaymentHistory ?? (_recurringPaymentHistory = new List<RecurringPaymentHistory>()); }
            protected set { _recurringPaymentHistory = value; }
        }        

        /// <summary>
        /// Gets the initial order
        /// </summary>
        public virtual Order InitialOrder { get; set; }
    }
}
