using Grand.Domain;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer report service interface
    /// </summary>
    public partial interface ICustomerReportService
    {
        /// <summary>
        /// Get best customers
        /// </summary>
        /// <param name="storeId">Store ident</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Order shipment status; null to load all records</param>
        /// <param name="orderBy">1 - order by order total, 2 - order by number of orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Report</returns>
        IPagedList<BestCustomerReportLine> GetBestCustomersReport(string storeId, DateTime? createdFromUtc,
            DateTime? createdToUtc, OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss, int orderBy,
            int pageIndex = 0, int pageSize = 214748364);

        /// Gets a report of customers registered in the last days
        /// <param name="storeId">Store ident</param>
        /// <param name="days">Customers registered in the last days</param>
        /// <returns>Number of registered customers</returns>
        Task<int> GetRegisteredCustomersReport(string storeId, int days);

        /// <summary>
        /// Get "customer by time" report
        /// </summary>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <returns>Result</returns>
        Task<IList<CustomerByTimeReportLine>> GetCustomerByTimeReport(string storeId, DateTime? startTimeUtc = null,
            DateTime? endTimeUtc = null);
    }
}