namespace Grand.Domain.Documents
{
    public enum DocumentStatus
    {
        /// <summary>
        /// Open 
        /// </summary>
        Open = 0,
        /// <summary>
        /// Processing
        /// </summary>
        Processing = 10,
        /// <summary>
        /// Approved
        /// </summary>
        Approved = 20,
        /// <summary>
        /// Closed
        /// </summary>
        Closed = 30,
        /// <summary>
        /// Canceled
        /// </summary>
        Cancelled = 40
    }
}
