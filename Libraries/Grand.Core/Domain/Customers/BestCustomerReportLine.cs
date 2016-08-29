
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Customers
{

    /// <summary>
    /// Represents a best customer report line
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class BestCustomerReportLine
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the order total
        /// </summary>
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the order count
        /// </summary>
        public int OrderCount { get; set; }
    }
}
