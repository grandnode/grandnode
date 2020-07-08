using Grand.Core;
using Grand.Domain;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Localization;
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
        private const string CATEGORIES_BY_ID_KEY = "Grand.category.id-{0}";
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
        private const string CATEGORIES_BY_PARENT_CATEGORY_ID_KEY = "Grand.category.byparent-{0}-{1}-{2}-{3}-{4}";
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
        private const string PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY = "Grand.productcategory.allbycategoryid-{0}-{1}-{2}-{3}-{4}-{5}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CATEGORIES_PATTERN_KEY = "Grand.category.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTCATEGORIES_PATTERN_KEY = "Grand.productcategory.";

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

        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
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
        /// <param name="categoryRepository">Category repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public CategoryService(ICacheManager cacheManager,
            IRepository<Category> categoryRepository,
            IRepository<Product> productRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IMediator mediator,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            CatalogSettings catalogSettings)
        {
            _cacheManager = cacheManager;
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _workContext = workContext;
            _storeContext = storeContext;
            _mediator = mediator;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Utilities

        protected IList<Category> SortCategoriesForTree(IList<Category> source, string parentId = "", bool ignoreCategoriesWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var result = new List<Category>();

            foreach (var cat in source.Where(c => c.ParentCategoryId == parentId).ToList())
            {
                result.Add(cat);
                result.AddRange(SortCategoriesForTree(source, cat.Id, true));
            }
            if (!ignoreCategoriesWithoutExistingParent && result.Count != source.Count)
            {
                //find categories without parent in provided category source and insert them into result
                foreach (var cat in source)
                    if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                        result.Add(cat);
            }
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="category">Category</param>
        public virtual async Task DeleteCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            //reset a "Parent category" property of all child subcategories
            var subcategories = await GetAllCategoriesByParentCategoryId(category.Id, true);
            foreach (var subcategory in subcategories)
            {
                subcategory.ParentCategoryId = "";
                await UpdateCategory(subcategory);
            }

            var builder = Builders<Product>.Update;
            var updatefilter = builder.PullFilter(x => x.ProductCategories, y => y.CategoryId == category.Id);
            await _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            await _categoryRepository.DeleteAsync(category);

            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(category);
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        public virtual async Task<IPagedList<Category>> GetAllCategories(string categoryName = "", string storeId = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = from c in _categoryRepository.Table
                        select c;

            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!String.IsNullOrWhiteSpace(categoryName))
                query = query.Where(m => m.Name != null && m.Name.ToLower().Contains(categoryName.ToLower()));

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

            query = query.OrderBy(c => c.ParentCategoryId).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Name);
            var unsortedCategories = query.ToList();
            //sort categories
            var sortedCategories = SortCategoriesForTree(unsortedCategories);

            //paging
            return await Task.FromResult(new PagedList<Category>(sortedCategories, pageIndex, pageSize));
        }

        /// <summary>
        /// Gets all categories filtered by parent category identifier
        /// </summary>
        /// <param name="parentCategoryId">Parent category identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="includeAllLevels">A value indicating whether we should load all child levels</param>
        /// <returns>Categories</returns>
        public virtual async Task<IList<Category>> GetAllCategoriesByParentCategoryId(string parentCategoryId = "",
            bool showHidden = false, bool includeAllLevels = false)
        {
            var storeId = _storeContext.CurrentStore.Id;
            var customer = _workContext.CurrentCustomer;
            string key = string.Format(CATEGORIES_BY_PARENT_CATEGORY_ID_KEY, parentCategoryId, showHidden, customer.Id, storeId, includeAllLevels);
            return await _cacheManager.GetAsync(key, async () =>
            {
                var builder = Builders<Category>.Filter;
                var filter = builder.Where(c => c.ParentCategoryId == parentCategoryId);
                if (!showHidden)
                    filter = filter & builder.Where(c => c.Published);

                if (!showHidden && (!_catalogSettings.IgnoreAcl || !_catalogSettings.IgnoreStoreLimitations))
                {
                    if (!showHidden && !_catalogSettings.IgnoreAcl)
                    {
                        //ACL (access control list)
                        var allowedCustomerRolesIds = customer.GetCustomerRoleIds();
                        filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));

                    }
                    if (!_catalogSettings.IgnoreStoreLimitations)
                    {
                        //Store mapping
                        var currentStoreId = new List<string> { storeId };
                        filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
                    }

                }
                var categories = _categoryRepository.Collection.Find(filter).SortBy(x => x.DisplayOrder).ToList();
                if (includeAllLevels)
                {
                    var childCategories = new List<Category>();
                    //add child levels
                    foreach (var category in categories)
                    {
                        childCategories.AddRange(await GetAllCategoriesByParentCategoryId(category.Id, showHidden, includeAllLevels));
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
        public virtual async Task<IList<Category>> GetAllCategoriesDisplayedOnHomePage(bool showHidden = false)
        {
            var builder = Builders<Category>.Filter;
            var filter = builder.Eq(x => x.Published, true);
            filter = filter & builder.Eq(x => x.ShowOnHomePage, true);
            var query = _categoryRepository.Collection.Find(filter).SortBy(x => x.DisplayOrder);

            var categories = await query.ToListAsync();
            if (!showHidden)
            {
                categories = categories
                    .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
                    .ToList();
            }

            return categories;
        }

        /// <summary>
        /// Gets all categories displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        public virtual async Task<IList<Category>> GetAllCategoriesFeaturedProductsOnHomePage(bool showHidden = false)
        {
            var builder = Builders<Category>.Filter;
            var filter = builder.Eq(x => x.Published, true);
            filter = filter & builder.Eq(x => x.FeaturedProductsOnHomaPage, true);
            var query = _categoryRepository.Collection.Find(filter).SortBy(x => x.DisplayOrder);

            var categories = await query.ToListAsync();
            if (!showHidden)
            {
                categories = categories
                    .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
                    .ToList();
            }
            return categories;
        }

        /// <summary>
        /// Gets all categories displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        public virtual async Task<IList<Category>> GetAllCategoriesSearchBox()
        {
            var builder = Builders<Category>.Filter;
            var filter = builder.Eq(x => x.Published, true);
            filter = filter & builder.Eq(x => x.ShowOnSearchBox, true);
            var query = _categoryRepository.Collection.Find(filter).SortBy(x => x.SearchBoxDisplayOrder);
            var categories = (await query.ToListAsync())
                .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
                .ToList();

            return categories;
        }

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="categoryService">Category service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Category breadcrumb </returns>
        public virtual async Task<IList<Category>> GetCategoryBreadCrumb(Category category, bool showHidden = false)
        {
            var result = new List<Category>();

            //used to prevent circular references
            var alreadyProcessedCategoryIds = new List<string>();

            while (category != null && //not null                
                (showHidden || category.Published) && //published
                (showHidden || _aclService.Authorize(category)) && //ACL
                (showHidden || _storeMappingService.Authorize(category)) && //Store mapping
                !alreadyProcessedCategoryIds.Contains(category.Id)) //prevent circular references
            {
                result.Add(category);

                alreadyProcessedCategoryIds.Add(category.Id);

                category = await GetCategoryById(category.ParentCategoryId);
            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="allCategories">All categories</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Category breadcrumb </returns>
        public virtual IList<Category> GetCategoryBreadCrumb(Category category, IList<Category> allCategories, bool showHidden = false)
        {
            var result = new List<Category>();

            //used to prevent circular references
            var alreadyProcessedCategoryIds = new List<string>();

            while (category != null && //not null                
                (showHidden || category.Published) && //published
                (showHidden || _aclService.Authorize(category)) && //ACL
                (showHidden || _storeMappingService.Authorize(category)) && //Store mapping
                !alreadyProcessedCategoryIds.Contains(category.Id)) //prevent circular references
            {
                result.Add(category);

                alreadyProcessedCategoryIds.Add(category.Id);

                category = (from c in allCategories
                            where c.Id == category.ParentCategoryId
                            select c).FirstOrDefault();
            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public virtual async Task<string> GetFormattedBreadCrumb(Category category, string separator = ">>", string languageId = "")
        {
            string result = string.Empty;

            var breadcrumb = await GetCategoryBreadCrumb(category, true);
            for (int i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var categoryName = breadcrumb[i].GetLocalized(x => x.Name, languageId);
                result = String.IsNullOrEmpty(result)
                    ? categoryName
                    : string.Format("{0} {1} {2}", result, separator, categoryName);
            }

            return result;
        }
        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="allCategories">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public virtual string GetFormattedBreadCrumb(Category category,
            IList<Category> allCategories, string separator = ">>", string languageId = "")
        {
            string result = string.Empty;

            var breadcrumb = GetCategoryBreadCrumb(category, allCategories, true);
            for (int i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var categoryName = breadcrumb[i].GetLocalized(x => x.Name, languageId);
                result = String.IsNullOrEmpty(result)
                    ? categoryName
                    : string.Format("{0} {1} {2}", result, separator, categoryName);
            }

            return result;
        }

        /// <summary>
        /// Gets all categories by discount id
        /// </summary>
        /// <returns>Categories</returns>
        public virtual async Task<IList<Category>> GetAllCategoriesByDiscount(string discountId)
        {
            var query = from c in _categoryRepository.Table
                        where c.AppliedDiscounts.Any(x => x == discountId)
                        select c;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets a category
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>Category</returns>
        public virtual async Task<Category> GetCategoryById(string categoryId)
        {
            string key = string.Format(CATEGORIES_BY_ID_KEY, categoryId);
            return await _cacheManager.GetAsync(key, () => _categoryRepository.GetByIdAsync(categoryId));
        }

        /// <summary>
        /// Inserts category
        /// </summary>
        /// <param name="category">Category</param>
        public virtual async Task InsertCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            await _categoryRepository.InsertAsync(category);

            //cache
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(category);
        }

        /// <summary>
        /// Updates the category
        /// </summary>
        /// <param name="category">Category</param>
        public virtual async Task UpdateCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");
            if (String.IsNullOrEmpty(category.ParentCategoryId))
                category.ParentCategoryId = "";

            //validate category hierarchy
            var parentCategory = await GetCategoryById(category.ParentCategoryId);
            while (parentCategory != null)
            {
                if (category.Id == parentCategory.Id)
                {
                    category.ParentCategoryId = "";
                    break;
                }
                parentCategory = await GetCategoryById(parentCategory.ParentCategoryId);
            }

            await _categoryRepository.UpdateAsync(category);

            //cache
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(category);
        }

        /// <summary>
        /// Deletes a product category mapping
        /// </summary>
        /// <param name="productCategory">Product category</param>
        public virtual async Task DeleteProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException("productCategory");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductCategories, productCategory);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productCategory.ProductId), update);

            //cache
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productCategory.ProductId));

            //event notification
            await _mediator.EntityDeleted(productCategory);

        }

        /// <summary>
        /// Gets product category mapping collection
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product a category mapping collection</returns>
        public virtual async Task<IPagedList<ProductCategory>> GetProductCategoriesByCategoryId(string categoryId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (String.IsNullOrEmpty(categoryId))
                return new PagedList<ProductCategory>(new List<ProductCategory>(), pageIndex, pageSize);

            string key = string.Format(PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY, showHidden, categoryId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = _productRepository.Table.Where(x => x.ProductCategories.Any(y => y.CategoryId == categoryId));

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
                var query_productCategories = from prod in query
                                              from pc in prod.ProductCategories
                                              select new SerializeProductCategory {
                                                  CategoryId = pc.CategoryId,
                                                  DisplayOrder = pc.DisplayOrder,
                                                  Id = pc.Id,
                                                  ProductId = prod.Id,
                                                  IsFeaturedProduct = pc.IsFeaturedProduct,
                                              };

                query_productCategories = from pm in query_productCategories
                                          where pm.CategoryId == categoryId
                                          orderby pm.DisplayOrder
                                          select pm;

                return Task.FromResult(new PagedList<ProductCategory>(query_productCategories, pageIndex, pageSize));
            });
        }


        /// <summary>
        /// Inserts a product category mapping
        /// </summary>
        /// <param name="productCategory">>Product category mapping</param>
        public virtual async Task InsertProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException("productCategory");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductCategories, productCategory);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productCategory.ProductId), update);

            //cache
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productCategory.ProductId));

            //event notification
            await _mediator.EntityInserted(productCategory);
        }

        /// <summary>
        /// Updates the product category mapping 
        /// </summary>
        /// <param name="productCategory">>Product category mapping</param>
        public virtual async Task UpdateProductCategory(ProductCategory productCategory)
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

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveByPrefix(CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productCategory.ProductId));

            //event notification
            await _mediator.EntityUpdated(productCategory);
        }
        #endregion

        public class SerializeProductCategory : ProductCategory
        {

        }

    }
}
