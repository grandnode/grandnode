using Grand.Domain;
using Grand.Domain.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Product tag service interface
    /// </summary>
    public partial interface ICustomerTagService
    {
        /// <summary>
        /// Delete a customer tag
        /// </summary>
        /// <param name="productTag">Customer tag</param>
        Task DeleteCustomerTag(CustomerTag customerTag);

        /// <summary>
        /// Gets all customer tags
        /// </summary>
        /// <returns>Customer tags</returns>
        Task<IList<CustomerTag>> GetAllCustomerTags();

        /// <summary>
        /// Gets all customer for tag id
        /// </summary>
        /// <returns>Customers</returns>
        Task<IPagedList<Customer>> GetCustomersByTag(string customerTagId = "", int pageIndex = 0, int pageSize = 2147483647);
        /// <summary>
        /// Gets customer tag
        /// </summary>
        /// <param name="customerTagId">Customer tag identifier</param>
        /// <returns>Product tag</returns>
        Task<CustomerTag> GetCustomerTagById(string customerTagId);

        /// <summary>
        /// Gets customer tag by name
        /// </summary>
        /// <param name="name">Customer tag name</param>
        /// <returns>Customer tag</returns>
        Task<CustomerTag> GetCustomerTagByName(string name);

        /// <summary>
        /// Gets customer tags search by name
        /// </summary>
        /// <param name="name">Customer tags name</param>
        /// <returns>Customer tags</returns>
        Task<IList<CustomerTag>> GetCustomerTagsByName(string name);

        /// <summary>
        /// Inserts a customer tag
        /// </summary>
        /// <param name="customerTag">Customer tag</param>
        Task InsertCustomerTag(CustomerTag customerTag);

        /// <summary>
        /// Insert tag to a customer
        /// </summary>
        Task InsertTagToCustomer(string customerTagId, string customerId);

        /// <summary>
        /// Delete tag from a customer
        /// </summary>
        Task DeleteTagFromCustomer(string customerTagId, string customerId);

        /// <summary>
        /// Updates the customer tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        Task UpdateCustomerTag(CustomerTag customerTag);

        /// <summary>
        /// Get number of customers
        /// </summary>
        /// <param name="customerTagId">Customer tag identifier</param>
        /// <returns>Number of products</returns>
        Task<int> GetCustomerCount(string customerTagId);

        /// <summary>
        /// Gets customer tag products for customer tag
        /// </summary>
        /// <param name="customerTagId">Customer tag id</param>
        /// <returns>Customer tag products</returns>
        Task<IList<CustomerTagProduct>> GetCustomerTagProducts(string customerTagId);

        /// <summary>
        /// Gets customer tag products for customer tag
        /// </summary>
        /// <param name="customerTagId">Customer tag id</param>
        /// <param name="productId">Product id</param>
        /// <returns>Customer tag product</returns>
        Task<CustomerTagProduct> GetCustomerTagProduct(string customerTagId, string productId);

        /// <summary>
        /// Gets customer tag product
        /// </summary>
        /// <param name="Id">id</param>
        /// <returns>Customer tag product</returns>
        Task<CustomerTagProduct> GetCustomerTagProductById(string id);

        /// <summary>
        /// Inserts a customer tag product
        /// </summary>
        /// <param name="customerTagProduct">Customer tag product</param>
        Task InsertCustomerTagProduct(CustomerTagProduct customerTagProduct);

        /// <summary>
        /// Updates the customer tag product
        /// </summary>
        /// <param name="customerTagProduct">Customer tag product</param>
        Task UpdateCustomerTagProduct(CustomerTagProduct customerTagProduct);

        /// <summary>
        /// Delete a customer tag product
        /// </summary>
        /// <param name="customerTagProduct">Customer tag product</param>
        Task DeleteCustomerTagProduct(CustomerTagProduct customerTagProduct);

    }
}
