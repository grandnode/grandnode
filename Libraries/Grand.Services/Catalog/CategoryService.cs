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
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Core.Infrastructure;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Category service
    /// </summary>
    public partial class CategoryService : ICategoryService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : category ID
        /// </remarks>
        private const string CATEGORIES_BY_ID_KEY = "Nop.category.id-{0}";
        /// <summary>
        /// Key for caching 
        /// </summary>
        /// <remarks>
        /// {0} : parent category ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// {4} : include all levels (child)
        /// </remarks>
        private const string CATEGORIES_BY_PARENT_CATEGORY_ID_KEY = "Nop.category.byparent-{0}-{1}-{2}-{3}-{4}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : category ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        private const string PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY = "Nop.productcategory.allbycategoryid-{0}-{1}-{2}-{3}-{4}-{5}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : product ID
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        private const string PRODUCTCATEGORIES_ALLBYPRODUCTID_KEY = "Nop.productcategory.allbyproductid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CATEGORIES_PATTERN_KEY = "Nop.category.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTCATEGORIES_PATTERN_KEY = "Nop.productcategory.";

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

        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="categoryRepository">Category repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="aclRepository">ACL record repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public CategoryService(ICacheManager cacheManager,
            IRepository<Category> categoryRepository,
            IRepository<Product> productRepository,
            IRepository<AclRecord> aclRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            CatalogSettings catalogSettings,
            IProductService productService)
        {
            this._cacheManager = cacheManager;
            this._categoryRepository = categoryRepository;
            this._productRepository = productRepository;
            this._aclRepository = aclRepository;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._eventPublisher = eventPublisher;
            this._storeMappingService = storeMappingService;
            this._aclService = aclService;
            this._catalogSettings = catalogSettings;
            this._productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="category">Category</param>
        public virtual void DeleteCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            //reset a "Parent category" property of all child subcategories
            var subcategories = GetAllCategoriesByParentCategoryId(category.Id, true);
            foreach (var subcategory in subcategories)
            {
                subcategory.ParentCategoryId = "";
                UpdateCategory(subcategory);
            }

            var builder = Builders<Product>.Update;
            var updatefilter = builder.PullFilter(x => x.ProductCategories, y => y.CategoryId == category.Id);
            var result = _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;

            _categoryRepository.Delete(category);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(category);

        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        public virtual IPagedList<Category> GetAllCategories(string categoryName = "", string storeId = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = from c in _categoryRepository.Table
                        select c;

            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!String.IsNullOrWhiteSpace(categoryName))
                query = query.Where(m => m.Name != null && m.Name.ToLower().Contains(categoryName.ToLower()));

            query = query.OrderBy(c => c.ParentCategoryId).ThenBy(c => c.DisplayOrder);
            
            if ((!_catalogSettings.IgnoreAcl || (!String.IsNullOrEmpty(storeId) &&  !_catalogSettings.IgnoreStoreLimitations)))
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

                query = query.OrderBy(c => c.ParentCategoryId).ThenBy(c => c.DisplayOrder);
                
            }
            
            var unsortedCategories = query.ToList();

            //sort categories
            var sortedCategories = unsortedCategories.SortCategoriesForTree();

            //paging
            return new PagedList<Category>(sortedCategories, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all categories filtered by parent category identifier
        /// </summary>
        /// <param name="parentCategoryId">Parent category identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="includeAllLevels">A value indicating whether we should load all child levels</param>
        /// <returns>Categories</returns>
        public virtual IList<Category> GetAllCategoriesByParentCategoryId(string parentCategoryId = "",
            bool showHidden = false, bool includeAllLevels = false)
        {
            string key = string.Format(CATEGORIES_BY_PARENT_CATEGORY_ID_KEY, parentCategoryId, showHidden, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id, includeAllLevels);
            return _cacheManager.Get(key, () =>
            {
                var query = from c in _categoryRepository.Table
                            select c;

                if (!showHidden)
                    query = query.Where(c => c.Published);
                query = query.Where(c => c.ParentCategoryId == parentCategoryId);                
                query = query.OrderBy(c => c.DisplayOrder);

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
                   
                    query = query.OrderBy(c => c.DisplayOrder);
                    
                }

                var categories = query.ToList();
                if (includeAllLevels)
                {
                    var childCategories = new List<Category>();
                    //add child levels
                    foreach (var category in categories)
                    {
                        childCategories.AddRange(GetAllCategoriesByParentCategoryId(category.Id, showHidden, includeAllLevels));
                    }
                    categories.AddRange(childCategories);
                }

                return categories;
            });

        }
        
        /// <summary>
        /// Gets all categories displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        public virtual IList<Category> GetAllCategoriesDisplayedOnHomePage(bool showHidden = false)
        {
            var query = from c in _categoryRepository.Table
                        orderby c.DisplayOrder
                        where c.Published &&                        
                        c.ShowOnHomePage
                        select c;

            var categories = query.ToList();
            if (!showHidden)
            {
                categories = categories
                    .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
                    .ToList();
            }

            return categories;
        }

        /// <summary>
        /// Gets all categories by discount id
        /// </summary>
        /// <returns>Categories</returns>
        public virtual IList<Category> GetAllCategoriesByDiscount(string discountId)
        {
            var query = from c in _categoryRepository.Table
                        where c.AppliedDiscounts.Any(x=>x.Id == discountId)
                        select c;

            var categories = query.ToList();
            return categories;
        }

        /// <summary>
        /// Gets a category
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>Category</returns>
        public virtual Category GetCategoryById(string categoryId)
        {
            string key = string.Format(CATEGORIES_BY_ID_KEY, categoryId);
            return _cacheManager.Get(key, () => _categoryRepository.GetById(categoryId));
        }

        /// <summary>
        /// Inserts category
        /// </summary>
        /// <param name="category">Category</param>
        public virtual void InsertCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            _categoryRepository.Insert(category);

            //cache
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(category);
        }

        /// <summary>
        /// Updates the category
        /// </summary>
        /// <param name="category">Category</param>
        public virtual void UpdateCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");
            if (String.IsNullOrEmpty(category.ParentCategoryId))
                category.ParentCategoryId = "";

            //validate category hierarchy
            var parentCategory = GetCategoryById(category.ParentCategoryId);
            while (parentCategory != null)
            {
                if (category.Id == parentCategory.Id)
                {
                    category.ParentCategoryId = "";
                    break;
                }
                parentCategory = GetCategoryById(parentCategory.ParentCategoryId);
            }

            _categoryRepository.Update(category);

            //cache
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(category);
        }
        
        /// <summary>
        /// Deletes a product category mapping
        /// </summary>
        /// <param name="productCategory">Product category</param>
        public virtual void DeleteProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException("productCategory");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductCategories, productCategory);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productCategory.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productCategory.ProductId));

            //event notification
            _eventPublisher.EntityDeleted(productCategory);
            
        }

        /// <summary>
        /// Gets product category mapping collection
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product a category mapping collection</returns>
        public virtual IPagedList<ProductCategory> GetProductCategoriesByCategoryId(string categoryId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (String.IsNullOrEmpty(categoryId))
                return new PagedList<ProductCategory>(new List<ProductCategory>(), pageIndex, pageSize);

            string key = string.Format(PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY, showHidden, categoryId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from p in _productRepository.Collection.AsQueryable()
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
                             from pc in prod.ProductCategories
                             select new SerializeProductCategory
                             {
                                 CategoryId = pc.CategoryId,
                                 DisplayOrder = pc.DisplayOrder,
                                 Id = pc.Id,
                                 ProductId = prod.Id,
                                 IsFeaturedProduct = pc.IsFeaturedProduct,
                             };

                query2 = from pm in query2
                         where pm.CategoryId == categoryId
                         orderby pm.DisplayOrder
                         select pm;


                var productCategories = new PagedList<ProductCategory>(query2, pageIndex, pageSize);
                return productCategories;
            });
        }

        /// <summary>
        /// Gets a product category mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> Product category mapping collection</returns>
        public virtual IList<ProductCategory> GetProductCategoriesByProductId(string productId, string storeId, bool showHidden = false)
        {
            if (String.IsNullOrEmpty(productId))
                return new List<ProductCategory>();

            string key = string.Format(PRODUCTCATEGORIES_ALLBYPRODUCTID_KEY, showHidden, productId, _workContext.CurrentCustomer.Id, storeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _productRepository.Table
                            from c in pc.ProductCategories 
                            where pc.Id == productId 
                            orderby pc.DisplayOrder
                            select new SerializeProductCategory() {
                                 CategoryId = c.CategoryId,
                                 DisplayOrder = c.DisplayOrder,
                                 Id = c.Id,
                                 IsFeaturedProduct = c.IsFeaturedProduct,
                                 ProductId = pc.Id
                            };

                var allProductCategories = query.ToList();
                var result = new List<ProductCategory>();
                if (!showHidden)
                {
                    foreach (var pc in allProductCategories)
                    {
                        //ACL (access control list) and store mapping
                        var category = _categoryRepository.GetById(pc.CategoryId);
                        if (_aclService.Authorize(category) && _storeMappingService.Authorize(category, storeId))
                            result.Add(pc);
                    }
                }
                else
                {
                    //no filtering
                    result.AddRange(allProductCategories);
                }
                return result;
            });
        }


        /// <summary>
        /// Inserts a product category mapping
        /// </summary>
        /// <param name="productCategory">>Product category mapping</param>
        public virtual void InsertProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException("productCategory");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductCategories, productCategory);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productCategory.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productCategory.ProductId));

            //event notification
            _eventPublisher.EntityInserted(productCategory);
        }

        /// <summary>
        /// Updates the product category mapping 
        /// </summary>
        /// <param name="productCategory">>Product category mapping</param>
        public virtual void UpdateProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException("productCategory");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, productCategory.ProductId);
            filter = filter & builder.Where(x => x.ProductCategories.Any(y => y.Id == productCategory.Id));
            var update = Builders<Product>.Update
                .Set(x => x.ProductCategories.ElementAt(-1).CategoryId, productCategory.CategoryId)
                .Set(x => x.ProductCategories.ElementAt(-1).IsFeaturedProduct, productCategory.IsFeaturedProduct)
                .Set(x => x.ProductCategories.ElementAt(-1).DisplayOrder, productCategory.DisplayOrder);

            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productCategory.ProductId));

            //event notification
            _eventPublisher.EntityUpdated(productCategory);
        }
        #endregion

        public class SerializeProductCategory: ProductCategory
        {

        }

    }
}
