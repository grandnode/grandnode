using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Services.Events;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MediatR;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product tag service
    /// </summary>
    public partial class ProductTagService : IProductTagService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        private const string PRODUCTTAG_COUNT_KEY = "Grand.producttag.count-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTTAG_PATTERN_KEY = "Grand.producttag.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>        
        private const string PRODUCTS_PATTERN_KEY = "Grand.product.";

        #endregion

        #region Fields

        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productTagRepository">Product tag repository</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="mediator">Mediator</param>
        public ProductTagService(IRepository<ProductTag> productTagRepository,
            IRepository<Product> productRepository,
            ICacheManager cacheManager,
            IMediator mediator
            )
        {
            _productTagRepository = productTagRepository;
            _cacheManager = cacheManager;
            _mediator = mediator;
            _productRepository = productRepository;
        }

        #endregion

        #region Nested classes

        private class ProductTagWithCount
        {
            public int ProductTagId { get; set; }
            public int ProductCount { get; set; }
        }

        #endregion
        
        #region Utilities

        /// <summary>
        /// Get product count for each of existing product tag
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Dictionary of "product tag ID : product count"</returns>
        private Dictionary<string, int> GetProductCount(string storeId)
        {
            string key = string.Format(PRODUCTTAG_COUNT_KEY, storeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pt in _productTagRepository.Table
                            select pt;

                var dictionary = new Dictionary<string, int>();
                foreach (var item in query.ToList())
                    dictionary.Add(item.Id, item.Count);
                return dictionary;

            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual async Task DeleteProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            var builder = Builders<Product>.Update;
            var updatefilter = builder.Pull(x => x.ProductTags, productTag.Name);
            await _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            await _productTagRepository.DeleteAsync(productTag);

            //cache
            await _cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(productTag);
        }

        /// <summary>
        /// Gets all product tags
        /// </summary>
        /// <returns>Product tags</returns>
        public virtual async Task<IList<ProductTag>> GetAllProductTags()
        {
            var query = _productTagRepository.Table;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets product tag
        /// </summary>
        /// <param name="productTagId">Product tag identifier</param>
        /// <returns>Product tag</returns>
        public virtual Task<ProductTag> GetProductTagById(string productTagId)
        {
            return _productTagRepository.GetByIdAsync(productTagId);
        }

        /// <summary>
        /// Gets product tag by name
        /// </summary>
        /// <param name="name">Product tag name</param>
        /// <returns>Product tag</returns>
        public virtual Task<ProductTag> GetProductTagByName(string name)
        {
            var query = from pt in _productTagRepository.Table
                        where pt.Name == name
                        select pt;

            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets product tag by sename
        /// </summary>
        /// <param name="sename">Product tag sename</param>
        /// <returns>Product tag</returns>
        public virtual Task<ProductTag> GetProductTagBySeName(string sename)
        {
            var query = from pt in _productTagRepository.Table
                        where pt.SeName == sename
                        select pt;
            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Inserts a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual async Task InsertProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            await _productTagRepository.InsertAsync(productTag);

            //cache
            await _cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(productTag);
        }

        /// <summary>
        /// Inserts a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual async Task UpdateProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            var previouse = await GetProductTagById(productTag.Id);

            await _productTagRepository.UpdateAsync(productTag);

            //update name on products
            var filter = new BsonDocument
            {
                new BsonElement("ProductTags", previouse.Name)
            };
            var update = Builders<Product>.Update
                .Set(x => x.ProductTags.ElementAt(-1), productTag.Name);
            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(productTag);
        }

        /// <summary>
        /// Get number of products
        /// </summary>
        /// <param name="productTagId">Product tag identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Number of products</returns>
        public virtual int GetProductCount(string productTagId, string storeId)
        {
            var dictionary = GetProductCount(storeId);
            if (dictionary.ContainsKey(productTagId))
                return dictionary[productTagId];
            
            return 0;
        }

        #endregion
    }
}
