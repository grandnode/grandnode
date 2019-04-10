using Grand.Core.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICategoryViewModelService
    {
        Task<CategoryListModel> PrepareCategoryListModel();
        Task<List<TreeNode>> PrepareCategoryNodeListModel();
        Task<(IEnumerable<CategoryModel> categoryListModel, int totalCount)> PrepareCategoryListModel(CategoryListModel model, int pageIndex, int pageSize);
        Task<CategoryModel> PrepareCategoryModel();
        Task<CategoryModel> PrepareCategoryModel(CategoryModel model, Category category);
        Task<Category> InsertCategoryModel(CategoryModel model);
        Task<Category> UpdateCategoryModel(Category category, CategoryModel model);
        Task DeleteCategory(Category category);
        Task<(IEnumerable<CategoryModel.CategoryProductModel> categoryProductModels, int totalCount)> PrepareCategoryProductModel(string categoryId, int pageIndex, int pageSize);
        Task<ProductCategory> UpdateProductCategoryModel(CategoryModel.CategoryProductModel model);
        Task DeleteProductCategoryModel(string id, string productId);
        Task<CategoryModel.AddCategoryProductModel> PrepareAddCategoryProductModel();
        Task InsertCategoryProductModel(CategoryModel.AddCategoryProductModel model);
        Task<(IEnumerable<CategoryModel.ActivityLogModel> activityLogModel, int totalCount)> PrepareActivityLogModel(string categoryId, int pageIndex, int pageSize);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CategoryModel.AddCategoryProductModel model, int pageIndex, int pageSize);
    }
}
