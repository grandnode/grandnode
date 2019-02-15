using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Discounts;
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
        private readonly CatalogSettings _catalogSettings;

        public CategoryViewModelService(ICategoryService categoryService, ICategoryTemplateService categoryTemplateService, IDiscountService discountService,
            ILocalizationService localizationService, IStoreService storeService, ICustomerService customerService, IPictureService pictureService,
            IUrlRecordService urlRecordService, ICustomerActivityService customerActivityService, IProductService productService, IManufacturerService manufacturerService,
            IVendorService vendorService, IDateTimeHelper dateTimeHelper, CatalogSettings catalogSettings)
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
            _catalogSettings = catalogSettings;
            _dateTimeHelper = dateTimeHelper;
        }

        protected virtual void PrepareAllCategoriesModel(CategoryModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCategories.Add(new SelectListItem
            {
                Text = "[None]",
                Value = ""
            });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
            {
                model.AvailableCategories.Add(new SelectListItem
                {
                    Text = c.GetFormattedBreadCrumb(categories),
                    Value = c.Id.ToString()
                });
            }
        }


        protected virtual void PrepareTemplatesModel(CategoryModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = _categoryTemplateService.GetAllCategoryTemplates();
            foreach (var template in templates)
            {
                model.AvailableCategoryTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }

        protected virtual void PrepareDiscountModel(CategoryModel model, Category category, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableDiscounts = _discountService
                .GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true)
                .Select(d => d.ToModel())
                .ToList();

            if (!excludeProperties && category != null)
            {
                model.SelectedDiscountIds = category.AppliedDiscounts.ToArray();
            }
        }

        protected void FillChildNodes(TreeNode parentNode, List<ITreeNode> nodes)
        {
            var children = nodes.Where(x => x.ParentCategoryId == parentNode.id);
            foreach (var child in children)
            {
                var newNode = new TreeNode
                {
                    id = child.Id,
                    text = child.Name,
                    nodes = new List<TreeNode>()
                };

                FillChildNodes(newNode, nodes);

                parentNode.nodes.Add(newNode);
            }
        }


        public virtual CategoryListModel PrepareCategoryListModel()
        {
            var model = new CategoryListModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            return model;
        }
        public virtual List<TreeNode> PrepareCategoryNodeListModel()
        {
            var categories = _categoryService.GetAllCategories();
            List<TreeNode> nodeList = new List<TreeNode>();
            List<ITreeNode> list = new List<ITreeNode>();
            list.AddRange(categories);
            foreach (var node in list)
            {
                if (string.IsNullOrEmpty(node.ParentCategoryId))
                {
                    var newNode = new TreeNode
                    {
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

        public virtual (IEnumerable<CategoryModel> categoryListModel, int totalCount) PrepareCategoryListModel(CategoryListModel model, int pageIndex, int pageSize)
        {
            var categories = _categoryService.GetAllCategories(model.SearchCategoryName, model.SearchStoreId,
                pageIndex - 1, pageSize, true);
            return (categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return categoryModel;
                }), categories.TotalCount);
        }

        public virtual CategoryModel PrepareCategoryModel()
        {
            var model = new CategoryModel();
            //templates
            PrepareTemplatesModel(model);
            //categories
            PrepareAllCategoriesModel(model);
            //discounts
            PrepareDiscountModel(model, null, true);
            //ACL
            model.PrepareACLModel(null, false, _customerService);
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);
            //default values
            model.PageSize = _catalogSettings.DefaultCategoryPageSize;
            model.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
            model.Published = true;
            model.IncludeInTopMenu = true;
            model.AllowCustomersToSelectPageSize = true;
            return model;
        }

        public virtual CategoryModel PrepareCategoryModel(CategoryModel model, Category category)
        {
            //templates
            PrepareTemplatesModel(model);
            //categories
            PrepareAllCategoriesModel(model);
            //discounts
            PrepareDiscountModel(model, category, false);
            return model;
        }

        public Category InsertCategoryModel(CategoryModel model)
        {
            var category = model.ToEntity();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    category.AppliedDiscounts.Add(discount.Id);
            }
            _categoryService.InsertCategory(category);

            //locales
            category.Locales = model.Locales.ToLocalizedProperty(category, x => x.Name, _urlRecordService);
            model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
            category.SeName = model.SeName;
            _categoryService.UpdateCategory(category);

            _urlRecordService.SaveSlug(category, model.SeName, "");

            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(category.PictureId, category.Name);

            //activity log
            _customerActivityService.InsertActivity("AddNewCategory", category.Id, _localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name);

            return category;
        }

        public virtual Category UpdateCategoryModel(Category category, CategoryModel model)
        {
            string prevPictureId = category.PictureId;
            category = model.ToEntity(category);
            category.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
            category.SeName = model.SeName;
            //locales
            category.Locales = model.Locales.ToLocalizedProperty(category, x => x.Name, _urlRecordService);
            _categoryService.UpdateCategory(category);
            //search engine name
            _urlRecordService.SaveSlug(category, model.SeName, "");

            //discounts
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
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
            _categoryService.UpdateCategory(category);
            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != category.PictureId)
            {
                var prevPicture = _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(category.PictureId, category.Name);

            //activity log
            _customerActivityService.InsertActivity("EditCategory", category.Id, _localizationService.GetResource("ActivityLog.EditCategory"), category.Name);
            return category;
        }
        public virtual void DeleteCategory(Category category)
        {
            _categoryService.DeleteCategory(category);
            //activity log
            _customerActivityService.InsertActivity("DeleteCategory", category.Id, _localizationService.GetResource("ActivityLog.DeleteCategory"), category.Name);
        }
        public virtual (IEnumerable<CategoryModel.CategoryProductModel> categoryProductModels, int totalCount) PrepareCategoryProductModel(string categoryId, int pageIndex, int pageSize)
        {
            var productCategories = _categoryService.GetProductCategoriesByCategoryId(categoryId,
                pageIndex - 1, pageSize, true);
            return (productCategories.Select(x => new CategoryModel.CategoryProductModel
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                ProductId = x.ProductId,
                ProductName = _productService.GetProductById(x.ProductId)?.Name,
                IsFeaturedProduct = x.IsFeaturedProduct,
                DisplayOrder = x.DisplayOrder
            }), productCategories.TotalCount);
        }

        public virtual ProductCategory UpdateProductCategoryModel(CategoryModel.CategoryProductModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            var productCategory = product.ProductCategories.FirstOrDefault(x => x.Id == model.Id);
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            productCategory.DisplayOrder = model.DisplayOrder;
            productCategory.ProductId = model.ProductId;
            _categoryService.UpdateProductCategory(productCategory);
            return productCategory;
        }
        public virtual void DeleteProductCategoryModel(string id, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productCategory = product.ProductCategories.Where(x => x.Id == id).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");
            productCategory.ProductId = productId;
            _categoryService.DeleteProductCategory(productCategory);

        }
        public virtual CategoryModel.AddCategoryProductModel PrepareAddCategoryProductModel()
        {
            var model = new CategoryModel.AddCategoryProductModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            return model;
        }

        public virtual void InsertCategoryProductModel(CategoryModel.AddCategoryProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = _productService.GetProductById(id);
                if (product != null)
                {
                    if (product.ProductCategories.Where(x => x.CategoryId == model.CategoryId).Count() == 0)
                    {
                        _categoryService.InsertProductCategory(
                            new ProductCategory
                            {
                                CategoryId = model.CategoryId,
                                ProductId = id,
                                IsFeaturedProduct = false,
                                DisplayOrder = 1,
                            });
                    }
                }
            }
        }
        public virtual (IEnumerable<CategoryModel.ActivityLogModel> activityLogModel, int totalCount) PrepareActivityLogModel(string categoryId, int pageIndex, int pageSize)
        {
            var activityLog = _customerActivityService.GetCategoryActivities(null, null, categoryId, pageIndex - 1, pageSize);
            return (activityLog.Select(x =>
                {
                    var customer = _customerService.GetCustomerById(x.CustomerId);
                    var m = new CategoryModel.ActivityLogModel
                    {
                        Id = x.Id,
                        ActivityLogTypeName = _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId)?.Name,
                        Comment = x.Comment,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                        CustomerId = x.CustomerId,
                        CustomerEmail = customer != null ? customer.Email : "null"
                    };
                    return m;

                }), activityLog.TotalCount);
        }
        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(CategoryModel.AddCategoryProductModel model, int pageIndex, int pageSize)
        {
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
    }
}
