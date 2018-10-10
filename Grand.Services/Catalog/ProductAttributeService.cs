using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Services.Events;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product attribute service
    /// </summary>
    public partial class ProductAttributeService : IProductAttributeService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : page index
        /// {1} : page size
        /// </remarks>
        private const string PRODUCTATTRIBUTES_ALL_KEY = "Grand.productattribute.all-{0}-{1}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product attribute ID
        /// </remarks>
        private const string PRODUCTATTRIBUTES_BY_ID_KEY = "Grand.productattribute.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTATTRIBUTEMAPPINGS_ALL_KEY = "Grand.productattributemapping.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product attribute mapping ID
        /// </remarks>
        private const string PRODUCTATTRIBUTEMAPPINGS_BY_ID_KEY = "Grand.productattributemapping.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product attribute mapping ID
        /// </remarks>
        private const string PRODUCTATTRIBUTEVALUES_ALL_KEY = "Grand.productattributevalue.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product attribute value ID
        /// </remarks>
        private const string PRODUCTATTRIBUTEVALUES_BY_ID_KEY = "Grand.productattributevalue.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTATTRIBUTECOMBINATIONS_ALL_KEY = "Grand.productattributecombination.all-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTATTRIBUTES_PATTERN_KEY = "Grand.productattribute.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTATTRIBUTEMAPPINGS_PATTERN_KEY = "Grand.productattributemapping.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTATTRIBUTEVALUES_PATTERN_KEY = "Grand.productattributevalue.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTATTRIBUTECOMBINATIONS_PATTERN_KEY = "Grand.productattributecombination.";

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

        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;


        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="productAttributeRepository">Product attribute repository</param>
        /// <param name="productAttributeCombinationRepository">Product attribute combination repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ProductAttributeService(ICacheManager cacheManager,
            IRepository<ProductAttribute> productAttributeRepository,            
            IRepository<Product> productRepository,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._productAttributeRepository = productAttributeRepository;
            this._productRepository = productRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        #region Product attributes

        /// <summary>
        /// Deletes a product attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute</param>
        public virtual void DeleteProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

            var builder = Builders<Product>.Update;
            var updatefilter = builder.PullFilter(x => x.ProductAttributeMappings, y => y.ProductAttributeId == productAttribute.Id);
            var result = _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;

            _productAttributeRepository.Delete(productAttribute);

            //cache

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productAttribute);
        }

        /// <summary>
        /// Gets all product attributes
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Product attributes</returns>
        public virtual IPagedList<ProductAttribute> GetAllProductAttributes(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            string key = string.Format(PRODUCTATTRIBUTES_ALL_KEY, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                var query = from pa in _productAttributeRepository.Table
                            orderby pa.Name
                            select pa;
                var productAttributes = new PagedList<ProductAttribute>(query, pageIndex, pageSize);
                return productAttributes;
            });
        }

        /// <summary>
        /// Gets a product attribute 
        /// </summary>
        /// <param name="productAttributeId">Product attribute identifier</param>
        /// <returns>Product attribute </returns>
        public virtual ProductAttribute GetProductAttributeById(string productAttributeId)
        {
            string key = string.Format(PRODUCTATTRIBUTES_BY_ID_KEY, productAttributeId);
            return _cacheManager.Get(key, () => _productAttributeRepository.GetById(productAttributeId));
        }

        /// <summary>
        /// Inserts a product attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute</param>
        public virtual void InsertProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

            _productAttributeRepository.Insert(productAttribute);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productAttribute);
        }

        /// <summary>
        /// Updates the product attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute</param>
        public virtual void UpdateProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

            _productAttributeRepository.Update(productAttribute);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productAttribute);
        }

        #endregion

        #region Product attributes mappings

        /// <summary>
        /// Deletes a product attribute mapping
        /// </summary>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        public virtual void DeleteProductAttributeMapping(ProductAttributeMapping productAttributeMapping)
        {
            if (productAttributeMapping == null)
                throw new ArgumentNullException("productAttributeMapping");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.PullFilter(p => p.ProductAttributeMappings, y=>y.Id == productAttributeMapping.Id);
            _productRepository.Collection.UpdateManyAsync(new BsonDocument("_id", productAttributeMapping.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productAttributeMapping.ProductId));

            //event notification
            _eventPublisher.EntityDeleted(productAttributeMapping);
        }

        /// <summary>
        /// Inserts a product attribute mapping
        /// </summary>
        /// <param name="productAttributeMapping">The product attribute mapping</param>
        public virtual void InsertProductAttributeMapping(ProductAttributeMapping productAttributeMapping)
        {
            if (productAttributeMapping == null)
                throw new ArgumentNullException("productAttributeMapping");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductAttributeMappings, productAttributeMapping);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productAttributeMapping.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productAttributeMapping.ProductId));

            //event notification
            _eventPublisher.EntityInserted(productAttributeMapping);
        }

        /// <summary>
        /// Updates the product attribute mapping
        /// </summary>
        /// <param name="productAttributeMapping">The product attribute mapping</param>
        public virtual void UpdateProductAttributeMapping(ProductAttributeMapping productAttributeMapping)
        {
            if (productAttributeMapping == null)
                throw new ArgumentNullException("productAttributeMapping");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, productAttributeMapping.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductAttributeMappings, y => y.Id == productAttributeMapping.Id);
            var update = Builders<Product>.Update
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).ProductAttributeId, productAttributeMapping.ProductAttributeId)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).TextPrompt, productAttributeMapping.TextPrompt)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).IsRequired, productAttributeMapping.IsRequired)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).AttributeControlTypeId, productAttributeMapping.AttributeControlTypeId)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).DisplayOrder, productAttributeMapping.DisplayOrder)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).ValidationMinLength, productAttributeMapping.ValidationMinLength)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).ValidationMaxLength, productAttributeMapping.ValidationMaxLength)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).ValidationFileAllowedExtensions, productAttributeMapping.ValidationFileAllowedExtensions)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).ValidationFileMaximumSize, productAttributeMapping.ValidationFileMaximumSize)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).DefaultValue, productAttributeMapping.DefaultValue)
                .Set(x => x.ProductAttributeMappings.ElementAt(-1).ConditionAttributeXml, productAttributeMapping.ConditionAttributeXml);

            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productAttributeMapping.ProductId));

            //event notification
            _eventPublisher.EntityUpdated(productAttributeMapping);
        }

        #endregion

        #region Product attribute values

        /// <summary>
        /// Deletes a product attribute value
        /// </summary>
        /// <param name="productAttributeValue">Product attribute value</param>
        public virtual void DeleteProductAttributeValue(ProductAttributeValue productAttributeValue)
        {
            if (productAttributeValue == null)
                throw new ArgumentNullException("productAttributeValue");

           var filter = Builders<Product>.Filter.And(Builders<Product>.Filter.Eq(x => x.Id, productAttributeValue.ProductId),
           Builders<Product>.Filter.ElemMatch(x => x.ProductAttributeMappings, x => x.Id == productAttributeValue.ProductAttributeMappingId));

            var p = _productRepository.GetById(productAttributeValue.ProductId);
            if (p != null)
            {
                var pavs = p.ProductAttributeMappings.Where(x => x.Id == productAttributeValue.ProductAttributeMappingId).FirstOrDefault();
                if (pavs != null)
                {
                    var pav = pavs.ProductAttributeValues.Where(x => x.Id == productAttributeValue.Id).FirstOrDefault();
                    if (pav != null)
                    {
                        pavs.ProductAttributeValues.Remove(pav);
                        var update = Builders<Product>.Update.Set("ProductAttributeMappings.$", pavs);
                        var result = _productRepository.Collection.UpdateOneAsync(filter, update).Result;

                    }
                }
            }

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productAttributeValue.ProductId));

            //event notification
            _eventPublisher.EntityDeleted(productAttributeValue);
        }


        /// <summary>
        /// Inserts a product attribute value
        /// </summary>
        /// <param name="productAttributeValue">The product attribute value</param>
        public virtual void InsertProductAttributeValue(ProductAttributeValue productAttributeValue)
        {
            if (productAttributeValue == null)
                throw new ArgumentNullException("productAttributeValue");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductAttributeMappings.ElementAt(-1).ProductAttributeValues, productAttributeValue);

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, productAttributeValue.ProductId);
            filter = filter & builder.Where(x => x.ProductAttributeMappings.Any(y => y.Id == productAttributeValue.ProductAttributeMappingId));

            var result = _productRepository.Collection.UpdateOneAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productAttributeValue.ProductId));

            //event notification
            _eventPublisher.EntityInserted(productAttributeValue);
        }

        /// <summary>
        /// Updates the product attribute value
        /// </summary>
        /// <param name="productAttributeValue">The product attribute value</param>
        public virtual void UpdateProductAttributeValue(ProductAttributeValue productAttributeValue)
        {
            if (productAttributeValue == null)
                throw new ArgumentNullException("productAttributeValue");


            var filter = Builders<Product>.Filter.And(Builders<Product>.Filter.Eq(x => x.Id, productAttributeValue.ProductId),
            Builders<Product>.Filter.ElemMatch(x => x.ProductAttributeMappings, x => x.Id == productAttributeValue.ProductAttributeMappingId));

            var p = _productRepository.GetById(productAttributeValue.ProductId);
            if (p!= null)
            {
                var pavs = p.ProductAttributeMappings.Where(x => x.Id == productAttributeValue.ProductAttributeMappingId).FirstOrDefault();
                if(pavs!=null)
                {
                    var pav = pavs.ProductAttributeValues.Where(x => x.Id == productAttributeValue.Id).FirstOrDefault();
                    if(pav!=null)
                    {
                        pav.AttributeValueTypeId = productAttributeValue.AttributeValueTypeId;
                        pav.AssociatedProductId = productAttributeValue.AssociatedProductId;
                        pav.Name = productAttributeValue.Name;
                        pav.ProductId = productAttributeValue.ProductId;
                        pav.ProductAttributeMappingId = productAttributeValue.ProductAttributeMappingId;
                        pav.ColorSquaresRgb = productAttributeValue.ColorSquaresRgb;
                        pav.ImageSquaresPictureId = productAttributeValue.ImageSquaresPictureId;
                        pav.PriceAdjustment = productAttributeValue.PriceAdjustment;
                        pav.WeightAdjustment = productAttributeValue.WeightAdjustment;
                        pav.Cost = productAttributeValue.Cost;
                        pav.Quantity = productAttributeValue.Quantity;
                        pav.IsPreSelected = productAttributeValue.IsPreSelected;
                        pav.DisplayOrder = productAttributeValue.DisplayOrder;
                        pav.PictureId = productAttributeValue.PictureId;
                        pav.Locales = productAttributeValue.Locales;

                        var update = Builders<Product>.Update.Set("ProductAttributeMappings.$", pavs);
                        var result = _productRepository.Collection.UpdateOneAsync(filter, update).Result;

                    }
                }
            }

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productAttributeValue.ProductId));

            //event notification
            _eventPublisher.EntityUpdated(productAttributeValue);
        }

        #endregion

        #region Predefined product attribute values

        /// <summary>
        /// Gets predefined product attribute values by product attribute identifier
        /// </summary>
        /// <param name="productAttributeId">The product attribute identifier</param>
        /// <returns>Product attribute mapping collection</returns>
        public virtual IList<PredefinedProductAttributeValue> GetPredefinedProductAttributeValues(string productAttributeId)
        {
            var query = from ppa in _productAttributeRepository.Table
                        where ppa.Id == productAttributeId
                        from ppav in ppa.PredefinedProductAttributeValues
                        select ppav;
            var values = query.OrderBy(x=> x.DisplayOrder).ToList();
            return values;
        }

        
        #endregion

        #region Product attribute combinations

        /// <summary>
        /// Deletes a product attribute combination
        /// </summary>
        /// <param name="combination">Product attribute combination</param>
        public virtual void DeleteProductAttributeCombination(ProductAttributeCombination combination)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductAttributeCombinations, combination);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", combination.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, combination.ProductId));

            //event notification
            _eventPublisher.EntityDeleted(combination);
        }

        /// <summary>
        /// Inserts a product attribute combination
        /// </summary>
        /// <param name="combination">Product attribute combination</param>
        public virtual void InsertProductAttributeCombination(ProductAttributeCombination combination)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductAttributeCombinations, combination);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", combination.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, combination.ProductId));

            //event notification
            _eventPublisher.EntityInserted(combination);
        }

        /// <summary>
        /// Updates a product attribute combination
        /// </summary>
        /// <param name="combination">Product attribute combination</param>
        public virtual void UpdateProductAttributeCombination(ProductAttributeCombination combination)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, combination.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductAttributeCombinations, y => y.Id == combination.Id);
            var update = Builders<Product>.Update
                .Set("ProductAttributeCombinations.$.StockQuantity", combination.StockQuantity)
                .Set("ProductAttributeCombinations.$.AllowOutOfStockOrders", combination.AllowOutOfStockOrders)
                .Set("ProductAttributeCombinations.$.Sku", combination.Sku)
                .Set("ProductAttributeCombinations.$.Text", combination.Text)
                .Set("ProductAttributeCombinations.$.ManufacturerPartNumber", combination.ManufacturerPartNumber)
                .Set("ProductAttributeCombinations.$.Gtin", combination.Gtin)
                .Set("ProductAttributeCombinations.$.OverriddenPrice", combination.OverriddenPrice)
                .Set("ProductAttributeCombinations.$.NotifyAdminForQuantityBelow", combination.NotifyAdminForQuantityBelow)
                .Set("ProductAttributeCombinations.$.WarehouseInventory", combination.WarehouseInventory)
                .Set("ProductAttributeCombinations.$.PictureId", combination.PictureId)
                .Set("ProductAttributeCombinations.$.TierPrices", combination.TierPrices);

            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, combination.ProductId));

            //event notification
            _eventPublisher.EntityUpdated(combination);
        }

        #endregion

        #endregion
    }
}
