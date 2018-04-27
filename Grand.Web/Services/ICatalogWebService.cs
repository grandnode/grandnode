using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Services
{
    public partial interface ICatalogWebService
    {
        Category GetCategoryById(string categoryId);
        List<string> GetChildCategoryIds(string parentCategoryId);
        void PrepareSortingOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command);
        void PrepareViewModes(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command);

        void PreparePageSizeOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize);

        List<CategorySimpleModel> PrepareCategorySimpleModels();
        List<CategorySimpleModel> PrepareCategorySimpleModels(string rootCategoryId,
            bool loadSubCategories = true, IList<Category> allCategories = null);

        CategoryNavigationModel PrepareCategoryNavigation(string currentCategoryId, string currentProductId);

        string PrepareCategoryTemplateViewPath(string templateId);

        CategoryModel PrepareCategory(Category category, CatalogPagingFilteringModel command);

        TopMenuModel PrepareTopMenu();

        List<CategoryModel> PrepareHomepageCategory();

        Manufacturer GetManufacturerById(string manufacturerId);
        string PrepareManufacturerTemplateViewPath(string templateId);
        ManufacturerModel PrepareManufacturer(Manufacturer manufacturer, CatalogPagingFilteringModel command);
        List<ManufacturerModel> PrepareManufacturerAll();

        List<ManufacturerModel> PrepareHomepageManufacturers();

        ManufacturerNavigationModel PrepareManufacturerNavigation(string currentManufacturerId);

        VendorModel PrepareVendor(Vendor vendor, CatalogPagingFilteringModel command);

        List<VendorModel> PrepareVendorAll();

        VendorNavigationModel PrepareVendorNavigation();

        ProductsByTagModel PrepareProductsByTag(ProductTag productTag, CatalogPagingFilteringModel command);

        PopularProductTagsModel PreparePopularProductTags();

        PopularProductTagsModel PrepareProductTagsAll();

        SearchBoxModel PrepareSearchBox();

        SearchModel PrepareSearch(SearchModel model, CatalogPagingFilteringModel command);


    }
}