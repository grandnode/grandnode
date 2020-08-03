using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain;
using Grand.Domain.Catalog;
using Grand.Domain.Data;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Security;
using Grand.Services.Stores;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private const string MANUFACTURERS_BY_ID_KEY = "Grand.manufacturer.id-{0}";
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
        private const string PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY = "Grand.productmanufacturer.allbymanufacturerid-{0}-{1}-{2}-{3}-{4}-{5}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MANUFACTURERS_PATTERN_KEY = "Grand.manufacturer.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTMANUFACTURERS_PATTERN_KEY = "Grand.productmanufacturer.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>        
        private const string PRODUCTS_PATTERN_KEY = "Grand.product.";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTS_BY_ID_KEY = "Grand.product.id-{0}";

        #endregion

        #region Fields

        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IWorkContext _workContext;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;

        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="manufacturerRepository">Category repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="mediator">Mediator</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="aclService">Acl service </param>
        public ManufacturerService(ICacheManager cacheManager,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IWorkContext workContext,
            CatalogSettings catalogSettings,
            IMediator mediator,
            IStoreMappingService storeMappingService,
            IAclService aclService)
        {
            _cacheManager = cacheManager;
            _manufacturerRepository = manufacturerRepository;
            _productRepository = productRepository;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
            _mediator = mediator;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Deletes a manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        public virtual async Task DeleteManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            var builder = Builders<Product>.Update;
            var updatefilter = builder.PullFilter(x => x.ProductManufacturers, y => y.ManufacturerId == manufacturer.Id);
            await _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(MANUFACTURERS_PATTERN_KEY);

            await _manufacturerRepository.DeleteAsync(manufacturer);

        }

        /// <summary>
        /// Gets all manufacturers
        /// </summary>
        /// <param name="manufacturerName">Manufacturer name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Manufacturers</returns>
        public virtual async Task<IPagedList<Manufacturer>> GetAllManufacturers(string manufacturerName = "",
            string storeId = "",
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            var query = from m in _manufacturerRepository.Table
                        select m;

            if (!showHidden)
                query = query.Where(m => m.Published);
            if (!String.IsNullOrWhiteSpace(manufacturerName))
                query = query.Where(m => m.Name != null && m.Name.ToLower().Contains(manufacturerName.ToLower()));

            if ((!_catalogSettings.IgnoreAcl || (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)))
            {
                if (!showHidden && !_catalogSettings.IgnoreAcl)
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
            }
            query = query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Name);
            return await PagedList<Manufacturer>.Create(query, pageIndex, pageSize);
        }


        /// <summary>
        /// Gets all manufacturers displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Manufacturers</returns>
        public virtual async Task<IList<Manufacturer>> GetAllManufacturerFeaturedProductsOnHomePage(bool showHidden = false)
        {
            var builder = Builders<Manufacturer>.Filter;
            var filter = builder.Eq(x => x.Published, true);
            filter = filter & builder.Eq(x => x.FeaturedProductsOnHomaPage, true);
            var query = _manufacturerRepository.Collection.Find(filter).SortBy(x => x.DisplayOrder);

            var manufacturers = await query.ToListAsync();
            if (!showHidden)
            {
                manufacturers = manufacturers
                    .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
                    .ToList();
            }
            return manufacturers;
        }


        /// <summary>
        /// Gets a manufacturer
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <returns>Manufacturer</returns>
        public virtual Task<Manufacturer> GetManufacturerById(string manufacturerId)
        {
            string key = string.Format(MANUFACTURERS_BY_ID_KEY, manufacturerId);
            return _cacheManager.GetAsync(key, () => _manufacturerRepository.GetByIdAsync(manufacturerId));
        }

        /// <summary>
        /// Inserts a manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        public virtual async Task InsertManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            await _manufacturerRepository.InsertAsync(manufacturer);

            //cache
            await _cacheManager.RemoveByPrefix(MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTMANUFACTURERS_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(manufacturer);
        }

        /// <summary>
        /// Updates the manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        public virtual async Task UpdateManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            await _manufacturerRepository.UpdateAsync(manufacturer);

            //cache
            await _cacheManager.RemoveByPrefix(MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTMANUFACTURERS_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(manufacturer);
        }

        /// <summary>
        /// Deletes a product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        public virtual async Task DeleteProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductManufacturers, productManufacturer);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productManufacturer.ProductId), update);

            //cache
            await _cacheManager.RemoveByPrefix(MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTMANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productManufacturer.ProductId));

            //event notification
            await _mediator.EntityDeleted(productManufacturer);
        }

        /// <summary>
        /// Gets product manufacturer collection
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product manufacturer collection</returns>
        public virtual async Task<IPagedList<ProductManufacturer>> GetProductManufacturersByManufacturerId(string manufacturerId, string storeId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            string key = string.Format(PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY, showHidden, manufacturerId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, storeId);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = _productRepository.Table.Where(x => x.ProductManufacturers.Any(y => y.ManufacturerId == manufacturerId));

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
                    if (!_catalogSettings.IgnoreStoreLimitations && !string.IsNullOrEmpty(storeId))
                    {
                        //Store mapping
                        query = from p in query
                                where !p.LimitedToStores || p.Stores.Contains(storeId)
                                select p;

                    }

                }

                var query_ProductManufacturer = from prod in query
                                                from pm in prod.ProductManufacturers
                                                select new SerializeProductManufacturer {
                                                    Id = pm.Id,
                                                    ProductId = prod.Id,
                                                    DisplayOrder = pm.DisplayOrder,
                                                    IsFeaturedProduct = pm.IsFeaturedProduct,
                                                    ManufacturerId = pm.ManufacturerId
                                                };

                query_ProductManufacturer = from pm in query_ProductManufacturer
                                            where pm.ManufacturerId == manufacturerId
                                            orderby pm.DisplayOrder
                                            select pm;

                return Task.FromResult(new PagedList<ProductManufacturer>(query_ProductManufacturer, pageIndex, pageSize));
            });
        }

        /// <summary>
        /// Gets a discount manufacturer mapping 
        /// </summary>
        /// <param name="discountId">Discount id mapping identifier</param>
        /// <returns>Product manufacturer mapping</returns>
        public virtual async Task<IList<Manufacturer>> GetAllManufacturersByDiscount(string discountId)
        {
            var query = from c in _manufacturerRepository.Table
                        where c.AppliedDiscounts.Any(x => x == discountId)
                        select c;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Inserts a product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        public virtual async Task InsertProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductManufacturers, productManufacturer);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productManufacturer.ProductId), update);

            //cache
            await _cacheManager.RemoveByPrefix(MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTMANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productManufacturer.ProductId));

            //event notification
            await _mediator.EntityInserted(productManufacturer);
        }

        /// <summary>
        /// Updates the product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        public virtual async Task UpdateProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, productManufacturer.ProductId);
            filter = filter & builder.Where(x => x.ProductManufacturers.Any(y => y.Id == productManufacturer.Id));
            var update = Builders<Product>.Update
                .Set(x => x.ProductManufacturers.ElementAt(-1).ManufacturerId, productManufacturer.ManufacturerId)
                .Set(x => x.ProductManufacturers.ElementAt(-1).IsFeaturedProduct, productManufacturer.IsFeaturedProduct)
                .Set(x => x.ProductManufacturers.ElementAt(-1).DisplayOrder, productManufacturer.DisplayOrder);

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveByPrefix(MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTMANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productManufacturer.ProductId));

            //event notification
            await _mediator.EntityUpdated(productManufacturer);
        }

        #endregion

        public class SerializeProductManufacturer : ProductManufacturer { }

    }
}
