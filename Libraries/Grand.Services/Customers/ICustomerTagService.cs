using System.Collections.Generic;
using Grand.Core.Domain.Customers;
using Grand.Core;

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
        void DeleteCustomerTag(CustomerTag customerTag);

        /// <summary>
        /// Gets all customer tags
        /// </summary>
        /// <returns>Customer tags</returns>
        IList<CustomerTag> GetAllCustomerTags();

        /// <summary>
        /// Gets all customer for tag id
        /// </summary>
        /// <returns>Customers</returns>
        IPagedList<Customer> GetCustomersByTag(string customerTagId = "", int pageIndex = 0, int pageSize = 2147483647);
        /// <summary>
        /// Gets customer tag
        /// </summary>
        /// <param name="customerTagId">Customer tag identifier</param>
        /// <returns>Product tag</returns>
        CustomerTag GetCustomerTagById(string customerTagId);

        /// <summary>
        /// Gets customer tag by name
        /// </summary>
        /// <param name="name">Customer tag name</param>
        /// <returns>Customer tag</returns>
        CustomerTag GetCustomerTagByName(string name);

        /// <summary>
        /// Gets customer tags search by name
        /// </summary>
        /// <param name="name">Customer tags name</param>
        /// <returns>Customer tags</returns>
        IList<CustomerTag> GetCustomerTagsByName(string name);

        /// <summary>
        /// Inserts a customer tag
        /// </summary>
        /// <param name="customerTag">Customer tag</param>
        void InsertCustomerTag(CustomerTag customerTag);

        /// <summary>
        /// Insert tag to a customer
        /// </summary>
        void InsertTagToCustomer(string customerTagId, string customerId);

        /// <summary>
        /// Delete tag from a customer
        /// </summary>
        void DeleteTagFromCustomer(string customerTagId, string customerId);

        /// <summary>
        /// Updates the customer tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        void UpdateCustomerTag(CustomerTag customerTag);

        /// <summary>
        /// Get number of customers
        /// </summary>
        /// <param name="customerTagId">Customer tag identifier</param>
        /// <returns>Number of products</returns>
        int GetCustomerCount(string customerTagId);
    }
}
