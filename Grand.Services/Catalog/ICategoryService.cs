using Grand.Domain;
using Grand.Domain.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public partial interface ICategoryService
    {
        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="category">Category</param>
        Task DeleteCategory(Category category);

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        Task<IPagedList<Category>> GetAllCategories(string categoryName = "", string storeId = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets all categories filtered by parent category identifier
        /// </summary>
        /// <param name="parentCategoryId">Parent category identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="includeAllLevels">A value indicating whether we should load all child levels</param>
        /// <returns>Categories</returns>
        Task<IList<Category>> GetAllCategoriesByParentCategoryId(string parentCategoryId = "",
            bool showHidden = false, bool includeAllLevels = false);

        /// <summary>
        /// Gets all categories displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        Task<IList<Category>> GetAllCategoriesDisplayedOnHomePage(bool showHidden = false);

        /// <summary>
        /// Gets all categories displayed on the home page - featured products
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        Task<IList<Category>> GetAllCategoriesFeaturedProductsOnHomePage(bool showHidden = false);

        /// <summary>
        /// Gets all categories displayed search box
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        Task<IList<Category>> GetAllCategoriesSearchBox();

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Category breadcrumb </returns>
        Task<IList<Category>> GetCategoryBreadCrumb(Category category, bool showHidden = false);

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="allCategories">All categories</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Category breadcrumb </returns>
        IList<Category> GetCategoryBreadCrumb(Category category, IList<Category> allCategories, bool showHidden = false);

        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        Task<string> GetFormattedBreadCrumb(Category category, string separator = ">>", string languageId = "");

        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="allCategories">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        string GetFormattedBreadCrumb(Category category,
            IList<Category> allCategories, string separator = ">>", string languageId = "");

        /// <summary>
        /// Gets all categories by discount id
        /// </summary>
        /// <returns>Categories</returns>
        Task<IList<Category>> GetAllCategoriesByDiscount(string discountId);

        /// <summary>
        /// Gets a category
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>Category</returns>
        Task<Category> GetCategoryById(string categoryId);

        /// <summary>
        /// Inserts category
        /// </summary>
        /// <param name="category">Category</param>
        Task InsertCategory(Category category);

        /// <summary>
        /// Updates the category
        /// </summary>
        /// <param name="category">Category</param>
        Task UpdateCategory(Category category);

        /// <summary>
        /// Deletes a product category mapping
        /// </summary>
        /// <param name="productCategory">Product category</param>
        Task DeleteProductCategory(ProductCategory productCategory);

        /// <summary>
        /// Gets product category mapping collection
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product a category mapping collection</returns>
        Task<IPagedList<ProductCategory>> GetProductCategoriesByCategoryId(string categoryId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Inserts a product category mapping
        /// </summary>
        /// <param name="productCategory">>Product category mapping</param>
        Task InsertProductCategory(ProductCategory productCategory);

        /// <summary>
        /// Updates the product category mapping 
        /// </summary>
        /// <param name="productCategory">>Product category mapping</param>
        Task UpdateProductCategory(ProductCategory productCategory);

        
    }
}
