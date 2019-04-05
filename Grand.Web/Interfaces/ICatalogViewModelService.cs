using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface ICatalogViewModelService
    {
        Task<Category> GetCategoryById(string categoryId);
        Task<List<string>> GetChildCategoryIds(string parentCategoryId);
        void PrepareSortingOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command);
        void PrepareViewModes(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command);

        void PreparePageSizeOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize);

        Task<List<CategorySimpleModel>> PrepareCategorySimpleModels();
        Task<List<CategorySimpleModel>> PrepareCategorySimpleModels(string rootCategoryId,
            bool loadSubCategories = true, IList<Category> allCategories = null);

        Task<CategoryNavigationModel> PrepareCategoryNavigation(string currentCategoryId, string currentProductId);

        Task<string> PrepareCategoryTemplateViewPath(string templateId);

        Task<CategoryModel> PrepareCategory(Category category, CatalogPagingFilteringModel command);

        Task<TopMenuModel> PrepareTopMenu();

        Task<List<CategoryModel>> PrepareHomepageCategory();
        Task<List<CategoryModel>> PrepareCategoryFeaturedProducts();

        Task<Manufacturer> GetManufacturerById(string manufacturerId);
        Task<string> PrepareManufacturerTemplateViewPath(string templateId);
        Task<ManufacturerModel> PrepareManufacturer(Manufacturer manufacturer, CatalogPagingFilteringModel command);
        Task<List<ManufacturerModel>> PrepareManufacturerFeaturedProducts();
        Task<List<ManufacturerModel>> PrepareManufacturerAll();

        Task<List<ManufacturerModel>> PrepareHomepageManufacturers();

        Task<ManufacturerNavigationModel> PrepareManufacturerNavigation(string currentManufacturerId);

        Task<VendorModel> PrepareVendor(Vendor vendor, CatalogPagingFilteringModel command);

        Task<List<VendorModel>> PrepareVendorAll();

        Task<VendorNavigationModel> PrepareVendorNavigation();

        Task<ProductsByTagModel> PrepareProductsByTag(ProductTag productTag, CatalogPagingFilteringModel command);

        Task<PopularProductTagsModel> PreparePopularProductTags();

        Task<PopularProductTagsModel> PrepareProductTagsAll();

        Task<IList<SearchAutoCompleteModel>> PrepareSearchAutoComplete(string term, string categoryId);

        Task<SearchBoxModel> PrepareSearchBox();

        Task<SearchModel> PrepareSearch(SearchModel model, CatalogPagingFilteringModel command);


    }
}