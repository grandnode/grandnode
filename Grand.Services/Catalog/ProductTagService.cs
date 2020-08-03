using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
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
        /// Key for all tags
        /// </summary>
        private const string PRODUCTTAG_ALL_KEY = "Grand.producttag.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTTAG_PATTERN_KEY = "Grand.producttag.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTS_BY_ID_KEY = "Grand.product.id-{0}";

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
        private async Task<Dictionary<string, int>> GetProductCount(string storeId)
        {
            string key = string.Format(PRODUCTTAG_COUNT_KEY, storeId);
            return await _cacheManager.GetAsync(key, async () =>
             {
                 var query = from pt in _productTagRepository.Table
                             select pt;

                 var dictionary = new Dictionary<string, int>();
                 foreach (var item in await query.ToListAsync())
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
            await _cacheManager.RemoveByPrefix(PRODUCTTAG_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(productTag);
        }

        /// <summary>
        /// Gets all product tags
        /// </summary>
        /// <returns>Product tags</returns>
        public virtual async Task<IList<ProductTag>> GetAllProductTags()
        {
            return await _cacheManager.GetAsync(PRODUCTTAG_ALL_KEY, async () =>
            {
                var query = _productTagRepository.Table;
                return await query.ToListAsync();
            });
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
            await _cacheManager.RemoveByPrefix(PRODUCTTAG_PATTERN_KEY);

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
            await _cacheManager.RemoveByPrefix(PRODUCTTAG_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(productTag);
        }

        /// <summary>
        /// Attach a tag to the product
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual async Task AttachProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductTags, productTag.Name);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productTag.ProductId), update);

            var builder = Builders<ProductTag>.Filter;
            var filter = builder.Eq(x => x.Id, productTag.Id);
            var updateTag = Builders<ProductTag>.Update
                .Inc(x => x.Count, 1);
            await _productTagRepository.Collection.UpdateManyAsync(filter, updateTag);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productTag.ProductId));

            //event notification
            await _mediator.EntityInserted(productTag);
        }

        /// <summary>
        /// Detach a tag from the product
        /// </summary>
        /// <param name="productTag">Product Tag</param>
        public virtual async Task DetachProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductTags, productTag.Name);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productTag.ProductId), update);

            var builder = Builders<ProductTag>.Filter;
            var filter = builder.Eq(x => x.Id, productTag.Id);
            var updateTag = Builders<ProductTag>.Update
                .Inc(x => x.Count, -1);
            await _productTagRepository.Collection.UpdateManyAsync(filter, updateTag);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productTag.ProductId));

            //event notification
            await _mediator.EntityDeleted(productTag);
        }


        /// <summary>
        /// Get number of products
        /// </summary>
        /// <param name="productTagId">Product tag identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Number of products</returns>
        public virtual async Task<int> GetProductCount(string productTagId, string storeId)
        {
            var dictionary = await GetProductCount(storeId);
            if (dictionary.ContainsKey(productTagId))
                return dictionary[productTagId];

            return 0;
        }

        #endregion
    }
}
