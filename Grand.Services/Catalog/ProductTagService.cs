using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Services.Events;
using MongoDB.Driver;
using MongoDB.Bson;

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
        private readonly IDataProvider _dataProvider;
        private readonly CommonSettings _commonSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productTagRepository">Product tag repository</param>
        /// <param name="dataProvider">Data provider</param>
        /// <param name="dbContext">Database Context</param>
        /// <param name="commonSettings">Common settings</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="eventPublisher">Event published</param>
        public ProductTagService(IRepository<ProductTag> productTagRepository,
            IRepository<Product> productRepository,
            IDataProvider dataProvider, 
            CommonSettings commonSettings,
            ICacheManager cacheManager,
            IEventPublisher eventPublisher
            )
        {
            this._productTagRepository = productTagRepository;
            this._dataProvider = dataProvider;
            this._commonSettings = commonSettings;
            this._cacheManager = cacheManager;
            this._eventPublisher = eventPublisher;
            this._productRepository = productRepository;
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
        public virtual void DeleteProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            var builder = Builders<Product>.Update;
            var updatefilter = builder.Pull(x => x.ProductTags, productTag.Id);
            var result = _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;

            _productTagRepository.Delete(productTag);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productTag);
        }

        /// <summary>
        /// Gets all product tags
        /// </summary>
        /// <returns>Product tags</returns>
        public virtual IList<ProductTag> GetAllProductTags()
        {
            var query = _productTagRepository.Table;
            var productTags = query.ToList();
            return productTags;
        }

        /// <summary>
        /// Gets product tag
        /// </summary>
        /// <param name="productTagId">Product tag identifier</param>
        /// <returns>Product tag</returns>
        public virtual ProductTag GetProductTagById(string productTagId)
        {
            return _productTagRepository.GetById(productTagId);
        }

        /// <summary>
        /// Gets product tag by name
        /// </summary>
        /// <param name="name">Product tag name</param>
        /// <returns>Product tag</returns>
        public virtual ProductTag GetProductTagByName(string name)
        {
            var query = from pt in _productTagRepository.Table
                        where pt.Name == name
                        select pt;

            var productTag = query.FirstOrDefault();
            return productTag;
        }

        /// <summary>
        /// Inserts a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual void InsertProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            _productTagRepository.Insert(productTag);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productTag);
        }

        /// <summary>
        /// Inserts a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual void UpdateProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            _productTagRepository.Update(productTag);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productTag);
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
