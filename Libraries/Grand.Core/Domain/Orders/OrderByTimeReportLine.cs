namespace Grand.Core.Domain.Orders
{
    /// <summary>
    /// Represents an "order by country" report line
    /// </summary>
    public partial class OrderByTimeReportLine
    {
        public string Time { get; set; }

        public int TotalOrders { get; set; }

        public decimal SumOrders { get; set; }
    }
}
