using Grand.Domain.Catalog;
using Grand.Domain.Discounts;
using Grand.Domain.Seo;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CategoryViewModelService : ICategoryViewModelService
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly CatalogSettings _catalogSettings;
        private readonly SeoSettings _seoSettings;

        public CategoryViewModelService(ICategoryService categoryService, ICategoryTemplateService categoryTemplateService, IDiscountService discountService,
            ILocalizationService localizationService, IStoreService storeService, ICustomerService customerService, IPictureService pictureService,
            IUrlRecordService urlRecordService, ICustomerActivityService customerActivityService, IProductService productService, IManufacturerService manufacturerService,
            IVendorService vendorService, IDateTimeHelper dateTimeHelper, ILanguageService languageService, CatalogSettings catalogSettings, SeoSettings seoSettings)
        {
            _categoryService = categoryService;
            _categoryTemplateService = categoryTemplateService;
            _discountService = discountService;
            _localizationService = localizationService;
            _storeService = storeService;
            _customerService = customerService;
            _urlRecordService = urlRecordService;
            _customerActivityService = customerActivityService;
            _productService = productService;
            _pictureService = pictureService;
            _manufacturerService = manufacturerService;
            _vendorService = vendorService;
            _languageService = languageService;
            _catalogSettings = catalogSettings;
            _dateTimeHelper = dateTimeHelper;
            _seoSettings = seoSettings;
        }

        protected virtual async Task PrepareAllCategoriesModel(CategoryModel model, string storeId)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCategories.Add(new SelectListItem {
                Text = "[None]",
                Value = ""
            });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
            {
                model.AvailableCategories.Add(new SelectListItem {
                    Text = _categoryService.GetFormattedBreadCrumb(c, categories),
                    Value = c.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareTemplatesModel(CategoryModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = await _categoryTemplateService.GetAllCategoryTemplates();
            foreach (var template in templates)
            {
                model.AvailableCategoryTemplates.Add(new SelectListItem {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareDiscountModel(CategoryModel model, Category category, bool excludeProperties, string storeId)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableDiscounts = (await _discountService
                .GetAllDiscounts(DiscountType.AssignedToCategories, storeId: storeId, showHidden: true))
                .Select(d => d.ToModel(_dateTimeHelper))
                .ToList();

            if (!excludeProperties && category != null)
            {
                model.SelectedDiscountIds = category.AppliedDiscounts.ToArray();
            }
        }
        protected virtual void PrepareSortOptionsModel(CategoryModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableSortOptions = ProductSortingEnum.Position.ToSelectList().ToList();
            model.AvailableSortOptions.Insert(0, new SelectListItem { Text = "None", Value = "-1" });
        }

        protected void FillChildNodes(TreeNode parentNode, List<ITreeNode> nodes)
        {
            var children = nodes.Where(x => x.ParentCategoryId == parentNode.id);
            foreach (var child in children)
            {
                var newNode = new TreeNode {
                    id = child.Id,
                    text = child.Name,
                    nodes = new List<TreeNode>()
                };

                FillChildNodes(newNode, nodes);

                parentNode.nodes.Add(newNode);
            }
        }


        public virtual async Task<CategoryListModel> PrepareCategoryListModel(string storeId)
        {
            var model = new CategoryListModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });
            return model;
        }
        public virtual async Task<List<TreeNode>> PrepareCategoryNodeListModel(string storeId)
        {
            var categories = await _categoryService.GetAllCategories(storeId: storeId, showHidden: true);
            var nodeList = new List<TreeNode>();
            var list = new List<ITreeNode>();
            list.AddRange(categories);
            foreach (var node in list)
            {
                if (string.IsNullOrEmpty(node.ParentCategoryId))
                {
                    var newNode = new TreeNode {
                        id = node.Id,
                        text = node.Name,
                        nodes = new List<TreeNode>()
                    };
                    FillChildNodes(newNode, list);
                    nodeList.Add(newNode);
                }
            }
            return nodeList;
        }

        public virtual async Task<(IEnumerable<CategoryModel> categoryListModel, int totalCount)> PrepareCategoryListModel(CategoryListModel model, int pageIndex, int pageSize)
        {
            var categories = await _categoryService.GetAllCategories(model.SearchCategoryName, model.SearchStoreId,
                pageIndex - 1, pageSize, true);

            var categoryListModel = new List<CategoryModel>();
            foreach (var x in categories)
            {
                var categoryModel = x.ToModel();
                categoryModel.Breadcrumb = await _categoryService.GetFormattedBreadCrumb(x);
                categoryListModel.Add(categoryModel);
            }
            return (categoryListModel, categories.TotalCount);
        }

        public virtual async Task<CategoryModel> PrepareCategoryModel(string storeId)
        {
            var model = new CategoryModel();
            //sort options
            PrepareSortOptionsModel(model);
            //templates
            await PrepareTemplatesModel(model);
            //categories
            await PrepareAllCategoriesModel(model, storeId);
            //discounts
            await PrepareDiscountModel(model, null, true, storeId);
            //ACL
            await model.PrepareACLModel(null, false, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false, storeId);

            //default values
            model.PageSize = _catalogSettings.DefaultCategoryPageSize;
            model.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
            model.Published = true;
            model.IncludeInTopMenu = true;
            model.AllowCustomersToSelectPageSize = true;
            return model;
        }

        public virtual async Task<CategoryModel> PrepareCategoryModel(CategoryModel model, Category category, string storeId)
        {
            //sort options
            PrepareSortOptionsModel(model);

            //templates
            await PrepareTemplatesModel(model);
            //categories
            await PrepareAllCategoriesModel(model, storeId);
            //discounts
            await PrepareDiscountModel(model, category, false, storeId);
            return model;
        }

        public async Task<Category> InsertCategoryModel(CategoryModel model)
        {
            var category = model.ToEntity();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    category.AppliedDiscounts.Add(discount.Id);
            }
            await _categoryService.InsertCategory(category);

            //locales
            category.Locales = await model.Locales.ToLocalizedProperty(category, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            model.SeName = await category.ValidateSeName(model.SeName, category.Name, true, _seoSettings, _urlRecordService, _languageService);
            category.SeName = model.SeName;
            await _categoryService.UpdateCategory(category);

            await _urlRecordService.SaveSlug(category, model.SeName, "");

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(category.PictureId, category.Name);

            //activity log
            await _customerActivityService.InsertActivity("AddNewCategory", category.Id, _localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name);

            return category;
        }

        public virtual async Task<Category> UpdateCategoryModel(Category category, CategoryModel model)
        {
            string prevPictureId = category.PictureId;
            category = model.ToEntity(category);
            category.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = await category.ValidateSeName(model.SeName, category.Name, true, _seoSettings, _urlRecordService, _languageService);
            category.SeName = model.SeName;
            //locales
            category.Locales = await model.Locales.ToLocalizedProperty(category, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            await _categoryService.UpdateCategory(category);
            //search engine name
            await _urlRecordService.SaveSlug(category, model.SeName, "");

            //discounts
            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (category.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                        category.AppliedDiscounts.Add(discount.Id);
                }
                else
                {
                    //remove discount
                    if (category.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                        category.AppliedDiscounts.Remove(discount.Id);
                }
            }
            await _categoryService.UpdateCategory(category);
            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != category.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(category.PictureId, category.Name);

            //activity log
            await _customerActivityService.InsertActivity("EditCategory", category.Id, _localizationService.GetResource("ActivityLog.EditCategory"), category.Name);
            return category;
        }
        public virtual async Task DeleteCategory(Category category)
        {
            await _categoryService.DeleteCategory(category);
            //activity log
            await _customerActivityService.InsertActivity("DeleteCategory", category.Id, _localizationService.GetResource("ActivityLog.DeleteCategory"), category.Name);
        }
        public virtual async Task<(IEnumerable<CategoryModel.CategoryProductModel> categoryProductModels, int totalCount)> PrepareCategoryProductModel(string categoryId, int pageIndex, int pageSize)
        {
            var productCategories = await _categoryService.GetProductCategoriesByCategoryId(categoryId,
                pageIndex - 1, pageSize, true);

            var categoryproducts = new List<CategoryModel.CategoryProductModel>();
            foreach (var item in productCategories)
            {
                var pc = new CategoryModel.CategoryProductModel {
                    Id = item.Id,
                    CategoryId = item.CategoryId,
                    ProductId = item.ProductId,
                    ProductName = (await _productService.GetProductById(item.ProductId))?.Name,
                    IsFeaturedProduct = item.IsFeaturedProduct,
                    DisplayOrder = item.DisplayOrder
                };
                categoryproducts.Add(pc);
            }
            return (categoryproducts, productCategories.TotalCount);
        }

        public virtual async Task<ProductCategory> UpdateProductCategoryModel(CategoryModel.CategoryProductModel model)
        {
            var product = await _productService.GetProductById(model.ProductId);
            var productCategory = product.ProductCategories.FirstOrDefault(x => x.Id == model.Id);
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            productCategory.DisplayOrder = model.DisplayOrder;
            productCategory.ProductId = model.ProductId;
            await _categoryService.UpdateProductCategory(productCategory);
            return productCategory;
        }
        public virtual async Task DeleteProductCategoryModel(string id, string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productCategory = product.ProductCategories.Where(x => x.Id == id).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");
            productCategory.ProductId = productId;
            await _categoryService.DeleteProductCategory(productCategory);

        }
        public virtual async Task<CategoryModel.AddCategoryProductModel> PrepareAddCategoryProductModel(string storeId)
        {
            var model = new CategoryModel.AddCategoryProductModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList().ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            return model;
        }

        public virtual async Task InsertCategoryProductModel(CategoryModel.AddCategoryProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    if (product.ProductCategories.Where(x => x.CategoryId == model.CategoryId).Count() == 0)
                    {
                        await _categoryService.InsertProductCategory(
                            new ProductCategory {
                                CategoryId = model.CategoryId,
                                ProductId = id,
                                IsFeaturedProduct = false,
                                DisplayOrder = 1,
                            });
                    }
                }
            }
        }
        public virtual async Task<(IEnumerable<CategoryModel.ActivityLogModel> activityLogModel, int totalCount)> PrepareActivityLogModel(string categoryId, int pageIndex, int pageSize)
        {
            var activityLog = await _customerActivityService.GetCategoryActivities(null, null, categoryId, pageIndex - 1, pageSize);
            var activityLogModelList = new List<CategoryModel.ActivityLogModel>();
            foreach (var item in activityLog)
            {
                var customer = await _customerService.GetCustomerById(item.CustomerId);
                var m = new CategoryModel.ActivityLogModel {
                    Id = item.Id,
                    ActivityLogTypeName = (await _customerActivityService.GetActivityTypeById(item.ActivityLogTypeId))?.Name,
                    Comment = item.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(item.CreatedOnUtc, DateTimeKind.Utc),
                    CustomerId = item.CustomerId,
                    CustomerEmail = customer != null ? customer.Email : "null"
                };
                activityLogModelList.Add(m);
            }
            return (activityLogModelList, activityLog.TotalCount);
        }
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CategoryModel.AddCategoryProductModel model, int pageIndex, int pageSize)
        {
            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
    }
}
