using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Messages;
using Grand.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.ExportImport
{
    /// <summary>
    /// Export manager interface
    /// </summary>
    public partial interface IExportManager
    {
        /// <summary>
        /// Export manufacturer list to xml
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>Result in XML format</returns>
        Task<string> ExportManufacturersToXml(IList<Manufacturer> manufacturers);

        /// <summary>
        /// Export manufacturers to XLSX
        /// </summary>
        /// <param name="manufacturers">Manufactures</param>
        byte[] ExportManufacturersToXlsx(IEnumerable<Manufacturer> manufacturers);
        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        Task<string> ExportCategoriesToXml(IEnumerable<Category> categories);

        /// <summary>
        /// Export category to XLSX
        /// </summary>
        /// <param name="categories">Categories</param>
        byte[] ExportCategoriesToXlsx(IEnumerable<Category> categories);
        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        Task<string> ExportProductsToXml(IList<Product> products);

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="products">Products</param>
        byte[] ExportProductsToXlsx(IEnumerable<Product> products);

        /// <summary>
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        Task<string> ExportOrdersToXml(IList<Order> orders);

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="orders">Orders</param>
        byte[] ExportOrdersToXlsx(IList<Order> orders);

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="customers">Customers</param>
        byte[] ExportCustomersToXlsx(IList<Customer> customers);

        /// <summary>
        /// Export customer - personal info to XLSX
        /// </summary>
        /// <param name="customer">Customer</param>
        Task<byte[]> ExportCustomerToXlsx(Customer customer, string stroreId);

        /// <summary>
        /// Export customer list to xml
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>Result in XML format</returns>
        Task<string> ExportCustomersToXml(IList<Customer> customers);

        /// <summary>
        /// Export newsletter subscribers to TXT
        /// </summary>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Result in TXT (string) format</returns>
        string ExportNewsletterSubscribersToTxt(IList<NewsLetterSubscription> subscriptions);

        /// <summary>
        /// Export newsletter subscribers to TXT
        /// </summary>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Result in TXT (string) format</returns>
        string ExportNewsletterSubscribersToTxt(IList<string> subscriptions);
        /// <summary>
        /// Export states to TXT
        /// </summary>
        /// <param name="states">States</param>
        /// <returns>Result in TXT (string) format</returns>
        Task<string> ExportStatesToTxt(IList<StateProvince> states);
    }
}
