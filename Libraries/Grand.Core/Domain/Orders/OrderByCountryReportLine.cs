namespace Grand.Core.Domain.Orders
{
    /// <summary>
    /// Represents an "order by country" report line
    /// </summary>
    public partial class OrderByCountryReportLine
    {
        /// <summary>
        /// Country identifier; null for unknow country
        /// </summary>
        public string CountryId { get; set; }

        /// <summary>
        /// Gets or sets the number of orders
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Gets or sets the order total summary
        /// </summary>
        public decimal SumOrders { get; set; }
    }
}
