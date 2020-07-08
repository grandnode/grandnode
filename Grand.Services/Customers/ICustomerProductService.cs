using Grand.Domain;
using Grand.Domain.Customers;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer product service interface
    /// </summary>
    public partial interface ICustomerProductService
    {
        #region Customer Product Price

        /// <summary>
        /// Gets a customer product price
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Customer product price</returns>
        Task<CustomerProductPrice> GetCustomerProductPriceById(string id);

        /// <summary>
        /// Gets a price
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <param name="productId">Product Identifier</param>
        /// <returns>Customer product price</returns>
        Task<decimal?> GetPriceByCustomerProduct(string customerId, string productId);

        /// <summary>
        /// Gets a customer product 
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Customer product</returns>
        Task<CustomerProduct> GetCustomerProduct(string id);

        /// <summary>
        /// Gets a customer product 
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <param name="productId">Product Identifier</param>
        /// <returns>Customer product</returns>
        Task<CustomerProduct> GetCustomerProduct(string customerId, string productId);

        /// <summary>
        /// Inserts a customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        Task InsertCustomerProductPrice(CustomerProductPrice customerProductPrice);

        /// <summary>
        /// Updates the customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        Task UpdateCustomerProductPrice(CustomerProductPrice customerProductPrice);

        /// <summary>
        /// Delete a customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        Task DeleteCustomerProductPrice(CustomerProductPrice customerProductPrice);



        /// <summary>
        /// Gets products price for customer
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <returns>Customer products price</returns>
        Task<IPagedList<CustomerProductPrice>> GetProductsPriceByCustomer(string customerId, int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion

        #region Customer product

        /// <summary>
        /// Inserts a customer product 
        /// </summary>
        /// <param name="customerProduct">Customer product</param>
        Task InsertCustomerProduct(CustomerProduct customerProduct);

        /// <summary>
        /// Updates the customer product
        /// </summary>
        /// <param name="customerProduct">Customer product </param>
        Task UpdateCustomerProduct(CustomerProduct customerProduct);

        /// <summary>
        /// Delete a customer product 
        /// </summary>
        /// <param name="customerProduct">Customer product </param>
        Task DeleteCustomerProduct(CustomerProduct customerProduct);

        Task<IPagedList<CustomerProduct>> GetProductsByCustomer(string customerId, int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion
    }
}
