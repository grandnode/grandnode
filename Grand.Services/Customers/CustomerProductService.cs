using Grand.Domain;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Customers;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver.Linq;
using System;
using System.Threading.Tasks;
using Grand.Core.Caching.Constants;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer product service interface
    /// </summary>
    public class CustomerProductService : ICustomerProductService
    {
        
        private readonly IRepository<CustomerProductPrice> _customerProductPriceRepository;
        private readonly IRepository<CustomerProduct> _customerProductRepository;
        private readonly ICacheBase _cacheBase;
        private readonly IMediator _mediator;

        public CustomerProductService(
            IRepository<CustomerProductPrice> customerProductPriceRepository,
            IRepository<CustomerProduct> customerProductRepository,
            ICacheBase cacheManager,
            IMediator mediator)
        {
            _cacheBase = cacheManager;
            _customerProductPriceRepository = customerProductPriceRepository;
            _customerProductRepository = customerProductRepository;
            _mediator = mediator;
        }

        #region Customer Product Price

        /// <summary>
        /// Gets a customer product price
        /// </summary>
        /// <param name="Id">Identifier</param>
        /// <returns>Customer product price</returns>
        public virtual Task<CustomerProductPrice> GetCustomerProductPriceById(string id)
        {
            return _customerProductPriceRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets a price
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <param name="productId">Product Identifier</param>
        /// <returns>Customer product price</returns>
        public virtual async Task<decimal?> GetPriceByCustomerProduct(string customerId, string productId)
        {
            var key = string.Format(CacheKey.CUSTOMER_PRODUCT_PRICE_KEY_ID, customerId, productId);
            var productprice = await _cacheBase.GetAsync(key, async () =>
            {
                var pp = await _customerProductPriceRepository.Table
                .Where(x => x.CustomerId == customerId && x.ProductId == productId)
                .FirstOrDefaultAsync();
                if (pp == null)
                    return (null, false);
                else
                    return (pp, true);
            });

            if (!productprice.Item2)
                return null;
            else
                return productprice.pp.Price;
        }

        /// <summary>
        /// Inserts a customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        public virtual async Task InsertCustomerProductPrice(CustomerProductPrice customerProductPrice)
        {
            if (customerProductPrice == null)
                throw new ArgumentNullException("customerProductPrice");

            await _customerProductPriceRepository.InsertAsync(customerProductPrice);

            //clear cache
            await _cacheBase.RemoveAsync(string.Format(CacheKey.CUSTOMER_PRODUCT_PRICE_KEY_ID, customerProductPrice.CustomerId, customerProductPrice.ProductId));

            //event notification
            await _mediator.EntityInserted(customerProductPrice);
        }

        /// <summary>
        /// Updates the customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        public virtual async Task UpdateCustomerProductPrice(CustomerProductPrice customerProductPrice)
        {
            if (customerProductPrice == null)
                throw new ArgumentNullException("customerProductPrice");

            await _customerProductPriceRepository.UpdateAsync(customerProductPrice);

            //clear cache
            await _cacheBase.RemoveAsync(string.Format(CacheKey.CUSTOMER_PRODUCT_PRICE_KEY_ID, customerProductPrice.CustomerId, customerProductPrice.ProductId));

            //event notification
            await _mediator.EntityUpdated(customerProductPrice);
        }

        /// <summary>
        /// Delete a customer product price
        /// </summary>
        /// <param name="customerProductPrice">Customer product price</param>
        public virtual async Task DeleteCustomerProductPrice(CustomerProductPrice customerProductPrice)
        {
            if (customerProductPrice == null)
                throw new ArgumentNullException("customerProductPrice");

            await _customerProductPriceRepository.DeleteAsync(customerProductPrice);

            //clear cache
            await _cacheBase.RemoveAsync(string.Format(CacheKey.CUSTOMER_PRODUCT_PRICE_KEY_ID, customerProductPrice.CustomerId, customerProductPrice.ProductId));

            //event notification
            await _mediator.EntityDeleted(customerProductPrice);
        }

        public virtual async Task<IPagedList<CustomerProductPrice>> GetProductsPriceByCustomer(string customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from pp in _customerProductPriceRepository.Table
                        where pp.CustomerId == customerId
                        select pp;
            return await PagedList<CustomerProductPrice>.Create(query, pageIndex, pageSize);
        }

        #endregion

        #region Personalize products

        /// <summary>
        /// Gets a customer product 
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Customer product</returns>
        public virtual async Task<CustomerProduct> GetCustomerProduct(string id)
        {
            return await _customerProductRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets a customer product 
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <param name="productId">Product Identifier</param>
        /// <returns>Customer product</returns>
        public virtual Task<CustomerProduct> GetCustomerProduct(string customerId, string productId)
        {
            var query = from pp in _customerProductRepository.Table
                        where pp.CustomerId == customerId && pp.ProductId == productId
                        select pp;

            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Insert a customer product 
        /// </summary>
        /// <param name="customerProduct">Customer product</param>
        public virtual async Task InsertCustomerProduct(CustomerProduct customerProduct)
        {
            if (customerProduct == null)
                throw new ArgumentNullException("customerProduct");

            await _customerProductRepository.InsertAsync(customerProduct);

            //clear cache
            await _cacheBase.RemoveAsync(string.Format(CacheKey.PRODUCTS_CUSTOMER_PERSONAL_KEY, customerProduct.CustomerId));

            //event notification
            await _mediator.EntityInserted(customerProduct);
        }

        /// <summary>
        /// Updates the customer product
        /// </summary>
        /// <param name="customerProduct">Customer product </param>
        public virtual async Task UpdateCustomerProduct(CustomerProduct customerProduct)
        {
            if (customerProduct == null)
                throw new ArgumentNullException("customerProduct");

            await _customerProductRepository.UpdateAsync(customerProduct);

            //clear cache
            await _cacheBase.RemoveAsync(string.Format(CacheKey.PRODUCTS_CUSTOMER_PERSONAL_KEY, customerProduct.CustomerId));

            //event notification
            await _mediator.EntityUpdated(customerProduct);
        }

        /// <summary>
        /// Delete a customer product 
        /// </summary>
        /// <param name="customerProduct">Customer product</param>
        public virtual async Task DeleteCustomerProduct(CustomerProduct customerProduct)
        {
            if (customerProduct == null)
                throw new ArgumentNullException("customerProduct");

            await _customerProductRepository.DeleteAsync(customerProduct);

            //clear cache
            await _cacheBase.RemoveAsync(string.Format(CacheKey.PRODUCTS_CUSTOMER_PERSONAL_KEY, customerProduct.CustomerId));

            //event notification
            await _mediator.EntityDeleted(customerProduct);
        }

        public virtual async Task<IPagedList<CustomerProduct>> GetProductsByCustomer(string customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from pp in _customerProductRepository.Table
                        where pp.CustomerId == customerId
                        orderby pp.DisplayOrder
                        select pp;
            return await PagedList<CustomerProduct>.Create(query, pageIndex, pageSize);
        }

        #endregion

    }
}
