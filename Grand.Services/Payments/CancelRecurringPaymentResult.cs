using System.Collections.Generic;

namespace Grand.Services.Payments
{
    /// <summary>
    /// Cancel recurring payment result
    /// </summary>
    public partial class CancelRecurringPaymentResult
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public CancelRecurringPaymentResult() 
        {
            Errors = new List<string>();
        }

        /// <summary>
        /// Gets a value indicating whether request has been completed successfully
        /// </summary>
        public bool Success
        {
            get { return (Errors.Count == 0); }
        }

        /// <summary>
        /// Add error
        /// </summary>
        /// <param name="error">Error</param>
        public void AddError(string error) 
        {
            Errors.Add(error);
        }

        /// <summary>
        /// Errors
        /// </summary>
        public IList<string> Errors { get; set; }
    }
}
