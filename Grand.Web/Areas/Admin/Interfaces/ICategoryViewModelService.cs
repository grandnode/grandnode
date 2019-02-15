using Grand.Core.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICategoryViewModelService
    {
        CategoryListModel PrepareCategoryListModel();
        List<TreeNode> PrepareCategoryNodeListModel();
        (IEnumerable<CategoryModel> categoryListModel, int totalCount) PrepareCategoryListModel(CategoryListModel model, int pageIndex, int pageSize);
        CategoryModel PrepareCategoryModel();
        CategoryModel PrepareCategoryModel(CategoryModel model, Category category);
        Category InsertCategoryModel(CategoryModel model);
        Category UpdateCategoryModel(Category category, CategoryModel model);
        void DeleteCategory(Category category);
        (IEnumerable<CategoryModel.CategoryProductModel> categoryProductModels, int totalCount) PrepareCategoryProductModel(string categoryId, int pageIndex, int pageSize);
        ProductCategory UpdateProductCategoryModel(CategoryModel.CategoryProductModel model);
        void DeleteProductCategoryModel(string id, string productId);
        CategoryModel.AddCategoryProductModel PrepareAddCategoryProductModel();
        void InsertCategoryProductModel(CategoryModel.AddCategoryProductModel model);
        (IEnumerable<CategoryModel.ActivityLogModel> activityLogModel, int totalCount) PrepareActivityLogModel(string categoryId, int pageIndex, int pageSize);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(CategoryModel.AddCategoryProductModel model, int pageIndex, int pageSize);
    }
}
