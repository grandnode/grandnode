using System.Collections.Generic;
using System.IO;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;

namespace Grand.Services.Common
{
    /// <summary>
    /// Customer service interface
    /// </summary>
    public partial interface IPdfService
    {
        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <returns>A path of generated file</returns>
        string PrintOrderToPdf(Order order, string languageId);

        /// <summary>
        /// Print orders to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        void PrintOrdersToPdf(Stream stream, IList<Order> orders, string languageId = "");

        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        void PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, string languageId = "");

        
        /// <summary>
        /// Print products to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        void PrintProductsToPdf(Stream stream, IList<Product> products);
    }
}