using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Stores;
using Grand.Services.Customers;
using Grand.Services.Events;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Manufacturer service
    /// </summary>
    public partial class ManufacturerService : IManufacturerService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer ID
        /// </remarks>
        private const string MANUFACTURERS_BY_ID_KEY = "Nop.manufacturer.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : manufacturer ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        private const string PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY = "Nop.productmanufacturer.allbymanufacturerid-{0}-{1}-{2}-{3}-{4}-{5}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : product ID
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        private const string PRODUCTMANUFACTURERS_ALLBYPRODUCTID_KEY = "Nop.productmanufacturer.allbyproductid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MANUFACTURERS_PATTERN_KEY = "Nop.manufacturer.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTMANUFACTURERS_PATTERN_KEY = "Nop.productmanufacturer.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>        
        private const string PRODUCTS_PATTERN_KEY = "Nop.product.";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTS_BY_ID_KEY = "Nop.product.id-{0}";

        #endregion

        #region Fields

        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        //private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="manufacturerRepository">Category repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="aclRepository">ACL record repository</param>
        /// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="eventPublisher">Event published</param>
        public ManufacturerService(ICacheManager cacheManager,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<AclRecord> aclRepository,
            //IRepository<StoreMapping> storeMappingRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._manufacturerRepository = manufacturerRepository;
            this._productRepository = productRepository;
            this._aclRepository = aclRepository;
            //this._storeMappingRepository = storeMappingRepository;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Deletes a manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        public virtual void DeleteManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            var builder = Builders<Product>.Update;
            var updatefilter = builder.PullFilter(x => x.ProductManufacturers, y => y.ManufacturerId == manufacturer.Id);
            var result = _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
                        
            _manufacturerRepository.Delete(manufacturer);

        }

        /// <summary>
        /// Gets all manufacturers
        /// </summary>
        /// <param name="manufacturerName">Manufacturer name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Manufacturers</returns>
        public virtual IPagedList<Manufacturer> GetAllManufacturers(string manufacturerName = "",
            string storeId = "",
            int pageIndex = 0,
            int pageSize = int.MaxValue, 
            bool showHidden = false)
        {
            var query = from m in _manufacturerRepository.Collection.AsQueryable()
                        select m;

            if (!showHidden)
                query = query.Where(m => m.Published);
            if (!String.IsNullOrWhiteSpace(manufacturerName))
                query = query.Where(m => m.Name != null && m.Name.ToLower().Contains(manufacturerName.ToLower()));
            
            query = query.OrderBy(m => m.DisplayOrder);

            if ((!_catalogSettings.IgnoreAcl || (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)))
            { 
                if (!_catalogSettings.IgnoreAcl)
                {
                    //ACL (access control list)
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    query = from p in query
                            where !p.SubjectToAcl || allowedCustomerRolesIds.Any(x => p.CustomerRoles.Contains(x))
                            select p;

                }
                if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    query = from p in query
                            where !p.LimitedToStores || p.Stores.Contains(storeId)
                            select p;
                }

                query = query.OrderBy(m => m.DisplayOrder);
            }

            return new PagedList<Manufacturer>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a manufacturer
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <returns>Manufacturer</returns>
        public virtual Manufacturer GetManufacturerById(string manufacturerId)
        {
            string key = string.Format(MANUFACTURERS_BY_ID_KEY, manufacturerId);
            return _cacheManager.Get(key, () => _manufacturerRepository.GetById(manufacturerId));
        }

        /// <summary>
        /// Inserts a manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        public virtual void InsertManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            _manufacturerRepository.Insert(manufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(manufacturer);
        }

        /// <summary>
        /// Updates the manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        public virtual void UpdateManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            _manufacturerRepository.Update(manufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(manufacturer);
        }

        /// <summary>
        /// Deletes a product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        public virtual void DeleteProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            //_productManufacturerRepository.Delete(productManufacturer);
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductManufacturers, productManufacturer);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productManufacturer.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productManufacturer.ProductId));

            //event notification
            _eventPublisher.EntityDeleted(productManufacturer);
        }

        /// <summary>
        /// Gets product manufacturer collection
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product manufacturer collection</returns>
        public virtual IPagedList<ProductManufacturer> GetProductManufacturersByManufacturerId(string manufacturerId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            string key = string.Format(PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY, showHidden, manufacturerId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from p in _productRepository.Table                            
                            select p;

                if (!showHidden && (!_catalogSettings.IgnoreAcl || !_catalogSettings.IgnoreStoreLimitations))
                {
                    if (!_catalogSettings.IgnoreAcl)
                    {
                        //ACL (access control list)
                        var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                        query = from p in query
                                where !p.SubjectToAcl || allowedCustomerRolesIds.Any(x => p.CustomerRoles.Contains(x))
                                select p;
                    }
                    if (!_catalogSettings.IgnoreStoreLimitations)
                    {
                        //Store mapping
                        var currentStoreId = _storeContext.CurrentStore.Id;
                        query = from p in query
                                where !p.LimitedToStores || p.Stores.Contains(currentStoreId)
                                select p;

                    }

                }

                var query2 = from prod in query
                             from pm in prod.ProductManufacturers
                             select new SerializeProductManufacturer
                                {
                                    Id = pm.Id,
                                    ProductId = prod.Id,
                                    DisplayOrder = pm.DisplayOrder,
                                    IsFeaturedProduct = pm.IsFeaturedProduct,
                                    ManufacturerId = pm.ManufacturerId    
                                };

                query2 = from pm in query2
                         where pm.ManufacturerId == manufacturerId
                         orderby pm.DisplayOrder
                         select pm;
                
                var productManufacturers = new PagedList<ProductManufacturer>(query2, pageIndex, pageSize);
                return productManufacturers;
            });
        }

        /// <summary>
        /// Gets a product manufacturer mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product manufacturer mapping collection</returns>
        public virtual IList<ProductManufacturer> GetProductManufacturersByProductId(string productId, bool showHidden = false)
        {
            string key = string.Format(PRODUCTMANUFACTURERS_ALLBYPRODUCTID_KEY, showHidden, productId, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from p in _productRepository.Table
                            from pm in p.ProductManufacturers
                            where pm.ProductId == productId 
                            orderby pm.DisplayOrder
                            select p;


                if (!showHidden && (!_catalogSettings.IgnoreAcl || !_catalogSettings.IgnoreStoreLimitations))
                {
                    if (!_catalogSettings.IgnoreAcl)
                    {
                        //ACL (access control list)
                        var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                        query = from p in query
                                where !p.SubjectToAcl || allowedCustomerRolesIds.Any(x => p.CustomerRoles.Contains(x))
                                select p;
                    }

                    if (!_catalogSettings.IgnoreStoreLimitations)
                    {
                        //Store mapping
                        var currentStoreId = _storeContext.CurrentStore.Id;
                        query = from p in query
                                where !p.LimitedToStores || p.Stores.Contains(currentStoreId)
                                select p;
                    }

                }
                var query2 = from p in query
                             from pm in p.ProductManufacturers
                             select new SerializeProductManufacturer
                             {
                                 Id = pm.Id,
                                 ProductId = p.Id,
                                 DisplayOrder = pm.DisplayOrder,
                                 IsFeaturedProduct = pm.IsFeaturedProduct,
                                 ManufacturerId = pm.ManufacturerId
                             };


                List<ProductManufacturer> productManufacturers = new List<ProductManufacturer>();
                productManufacturers.AddRange(query2.ToList());
                return productManufacturers;
            });
        }

        /// <summary>
        /// Gets a discount manufacturer mapping 
        /// </summary>
        /// <param name="discountId">Discount id mapping identifier</param>
        /// <returns>Product manufacturer mapping</returns>
        public virtual IList<Manufacturer> GetAllManufacturersByDiscount(string discountId)
        {
            var query = from c in _manufacturerRepository.Table
                        where c.AppliedDiscounts.Any(x=>x.Id == discountId)
                        select c;

            var manufacturers = query.ToList();
            return manufacturers;

        }

        /// <summary>
        /// Inserts a product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        public virtual void InsertProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            //_productManufacturerRepository.Insert(productManufacturer);
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductManufacturers, productManufacturer);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productManufacturer.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productManufacturer.ProductId));

            //event notification
            _eventPublisher.EntityInserted(productManufacturer);
        }

        /// <summary>
        /// Updates the product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        public virtual void UpdateProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            //_productManufacturerRepository.Update(productManufacturer);

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, productManufacturer.ProductId);
            filter = filter & builder.Where(x => x.ProductManufacturers.Any(y => y.Id == productManufacturer.Id));
            var update = Builders<Product>.Update
                .Set(x => x.ProductManufacturers.ElementAt(-1).ManufacturerId, productManufacturer.ManufacturerId)
                .Set(x => x.ProductManufacturers.ElementAt(-1).IsFeaturedProduct, productManufacturer.IsFeaturedProduct)
                .Set(x => x.ProductManufacturers.ElementAt(-1).DisplayOrder, productManufacturer.DisplayOrder);

            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productManufacturer.ProductId));

            //event notification
            _eventPublisher.EntityUpdated(productManufacturer);
        }

        #endregion

        public class SerializeProductManufacturer : ProductManufacturer { }

    }
}
