﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Catalog;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Discounts;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Discounts;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Framework;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;
using Grand.Core.Domain.Localization;
using MongoDB.Bson;
using Grand.Services.Helpers;

namespace Grand.Admin.Controllers
{
    public partial class CategoryController : BaseAdminController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IExportManager _exportManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IVendorService _vendorService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly IImportManager _importManager;
        private readonly CatalogSettings _catalogSettings;

        #endregion
        
        #region Constructors

        public CategoryController(ICategoryService categoryService, ICategoryTemplateService categoryTemplateService,
            IManufacturerService manufacturerService, IProductService productService, 
            ICustomerService customerService,
            IUrlRecordService urlRecordService, 
            IPictureService pictureService, 
            ILanguageService languageService,
            ILocalizationService localizationService, 
            IDiscountService discountService,
            IPermissionService permissionService,
            IAclService aclService, 
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IExportManager exportManager, 
            IVendorService vendorService, 
            ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            IImportManager importManager,
            CatalogSettings catalogSettings)
        {
            this._categoryService = categoryService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._discountService = discountService;
            this._permissionService = permissionService;
            this._vendorService = vendorService;
            this._aclService = aclService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._exportManager = exportManager;
            this._customerActivityService = customerActivityService;
            this._dateTimeHelper = dateTimeHelper;
            this._workContext = workContext;
            this._importManager = importManager;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(Category category, CategoryModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();

            foreach (var local in model.Locales)
            {
                var seName = category.ValidateSeName(local.SeName, local.Name, false);
                _urlRecordService.SaveSlug(category, seName, local.LanguageId);

                if (!(String.IsNullOrEmpty(seName)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "SeName",
                        LocaleValue = seName,
                    });

                if (!(String.IsNullOrEmpty(local.Description)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Description",
                        LocaleValue = local.Description,
                    });

                if (!(String.IsNullOrEmpty(local.MetaDescription)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaDescription",
                        LocaleValue = local.MetaDescription,
                    });

                if (!(String.IsNullOrEmpty(local.MetaKeywords)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaKeywords",
                        LocaleValue = local.MetaKeywords,
                    });

                if (!(String.IsNullOrEmpty(local.MetaTitle)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaTitle",
                        LocaleValue = local.MetaTitle,
                    });

                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });

            }
            return localized;
        }

        [NonAction]
        protected virtual void UpdatePictureSeoNames(Category category)
        {
            var picture = _pictureService.GetPictureById(category.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(category.Name));
        }

        [NonAction]
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

        [NonAction]
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

        [NonAction]
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
                model.SelectedDiscountIds = category.AppliedDiscounts.Select(d => d.Id).ToArray();
            }
        }

        [NonAction]
        protected virtual void PrepareAclModel(CategoryModel model, Category category, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (category != null)
                {
                    model.SelectedCustomerRoleIds = category.CustomerRoles.ToArray();
                }
            }
        }
        

        [NonAction]
        protected virtual void PrepareStoresMappingModel(CategoryModel model, Category category, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (category != null)
                {
                    model.SelectedStoreIds = category.Stores.ToArray();
                }
            }
        }
        
        #endregion
        
        #region List / tree

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var model = new CategoryListModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, CategoryListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var categories = _categoryService.GetAllCategories(model.SearchCategoryName, model.SearchStoreId,
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return categoryModel;
                }),
                Total = categories.TotalCount
            };
            return Json(gridModel);
        }
        
        public ActionResult Tree()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            return View();
        }

        [HttpPost,]
        public ActionResult TreeLoadChildren(string id = "")
        {
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(id, true)
                .Select(x => new
                             {
                                 id = x.Id,
                                 Name = x.Name,
                                 hasChildren = _categoryService.GetAllCategoriesByParentCategoryId(x.Id, true).Count > 0,
                             });

            return Json(categories);
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var model = new CategoryModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //templates
            PrepareTemplatesModel(model);
            //categories
            PrepareAllCategoriesModel(model);
            //discounts
            PrepareDiscountModel(model, null, true);
            //ACL
            PrepareAclModel(model, null, false);
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //default values
            model.PageSize = _catalogSettings.DefaultCategoryPageSize;
            model.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
            model.Published = true;
            model.IncludeInTopMenu = true;
            model.AllowCustomersToSelectPageSize = true;            

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var category = model.ToEntity();
                category.CreatedOnUtc = DateTime.UtcNow;
                category.UpdatedOnUtc = DateTime.UtcNow;

                category.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                category.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        category.AppliedDiscounts.Add(discount);
                }
                _categoryService.InsertCategory(category);

                //locales
                category.Locales = UpdateLocales(category, model);
                model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
                category.SeName = model.SeName;
                _categoryService.UpdateCategory(category);

                _urlRecordService.SaveSlug(category, model.SeName, "");

                //update picture seo file name
                UpdatePictureSeoNames(category);

                //activity log
                _customerActivityService.InsertActivity("AddNewCategory", category.Id, _localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = category.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
            //categories
            PrepareAllCategoriesModel(model);
            //discounts
            PrepareDiscountModel(model, null, true);
            //ACL
            PrepareAclModel(model, null, true);
            //Stores
            PrepareStoresMappingModel(model, null, true);
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = _categoryService.GetCategoryById(id);
            if (category == null) 
                //No category found with the specified id
                return RedirectToAction("List");

            var model = category.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = category.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = category.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = category.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = category.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = category.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = category.GetSeName(languageId, false, false);
            });
            //templates
            PrepareTemplatesModel(model);
            //categories
            PrepareAllCategoriesModel(model);
            //discounts
            PrepareDiscountModel(model, category, false);
            //ACL
            PrepareAclModel(model, category, false);
            //Stores
            PrepareStoresMappingModel(model, category, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = _categoryService.GetCategoryById(model.Id);
            if (category == null)
                //No category found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                string prevPictureId = category.PictureId;
                category = model.ToEntity(category);
                category.UpdatedOnUtc = DateTime.UtcNow;
                model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
                category.SeName = model.SeName;
                //locales
                category.Locales = UpdateLocales(category, model);
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
                        if (category.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                            category.AppliedDiscounts.Add(discount);
                    }
                    else
                    {
                        //remove discount
                        if (category.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                            category.AppliedDiscounts.Remove(discount);
                    }
                }
                category.CustomerRoles = model.SelectedCustomerRoleIds != null? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                category.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                _categoryService.UpdateCategory(category);
                //delete an old picture (if deleted or updated)
                if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != category.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }
                //update picture seo file name
                UpdatePictureSeoNames(category);

                //activity log
                _customerActivityService.InsertActivity("EditCategory", category.Id, _localizationService.GetResource("ActivityLog.EditCategory"), category.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = category.Id});
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
            //categories
            PrepareAllCategoriesModel(model);
            //discounts
            PrepareDiscountModel(model, category, true);
            //ACL
            PrepareAclModel(model, category, true);
            //Stores
            PrepareStoresMappingModel(model, category, true);

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = _categoryService.GetCategoryById(id);
            if (category == null)
                //No category found with the specified id
                return RedirectToAction("List");

            _categoryService.DeleteCategory(category);

            //activity log
            _customerActivityService.InsertActivity("DeleteCategory", category.Id, _localizationService.GetResource("ActivityLog.DeleteCategory"), category.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Deleted"));
            return RedirectToAction("List");
        }
        

        #endregion

        #region Export / Import

        public ActionResult ExportXml()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            try
            {
                var xml = _exportManager.ExportCategoriesToXml();
                return new XmlDownloadResult(xml, "categories.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public ActionResult ExportXlsx()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            try
            {
                var bytes = _exportManager.ExportCategoriesToXlsx(_categoryService.GetAllCategories(showHidden: true));

                return File(bytes, "text/xls", "categories.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public ActionResult ImportFromXlsx()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //a vendor cannot import categories
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            try
            {
                var file = Request.Files["importexcelfile"];
                if (file != null && file.ContentLength > 0)
                {
                    _importManager.ImportCategoryFromXlsx(file.InputStream);
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Category.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }
        #endregion

        #region Products

        [HttpPost]
        public ActionResult ProductList(DataSourceRequest command, string categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var productCategories = _categoryService.GetProductCategoriesByCategoryId(categoryId,
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = productCategories.Select(x => new CategoryModel.CategoryProductModel
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    ProductId = x.ProductId,
                    ProductName = _productService.GetProductById(x.ProductId).Name,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                }),
                Total = productCategories.TotalCount
            };

            return Json(gridModel);
        }

        public ActionResult ProductUpdate(CategoryModel.CategoryProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            var product = _productService.GetProductById(model.ProductId);
            var productCategory = product.ProductCategories.FirstOrDefault(x=>x.Id == model.Id);
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            productCategory.DisplayOrder = model.DisplayOrder;
            productCategory.ProductId = model.ProductId;
            _categoryService.UpdateProductCategory(productCategory);

            return new NullJsonResult();
        }

        public ActionResult ProductDelete(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productCategory = product.ProductCategories.Where(x => x.Id == id).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");
            productCategory.ProductId = productId;

            _categoryService.DeleteProductCategory(productCategory);

            return new NullJsonResult();
        }

        public ActionResult ProductAddPopup(string categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            
            var model = new CategoryModel.AddCategoryProductModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAddPopupList(DataSourceRequest command, CategoryModel.AddCategoryProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            var searchCategoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                searchCategoryIds.Add(model.SearchCategoryId);

            var gridModel = new DataSourceResult();
            var products = _productService.SearchProducts(
                categoryIds: searchCategoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            gridModel.Data = products.Select(x => x.ToModel());
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }
        
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult ProductAddPopup(string btnId, string formId, CategoryModel.AddCategoryProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
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

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion

        #region Activity log

        [HttpPost]
        public ActionResult ListActivityLog(DataSourceRequest command, string categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return Content("");

            var activityLog = _customerActivityService.GetCategoryActivities(null, null, categoryId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
                {
                    var customer = _customerService.GetCustomerById(x.CustomerId);
                    var m = new CategoryModel.ActivityLogModel
                    {
                        Id = x.Id,
                        ActivityLogTypeName = x.ActivityLogType.Name,
                        Comment = x.Comment,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                        CustomerId = x.CustomerId,
                        CustomerEmail = customer!=null? customer.Email : "null"
                    };
                    return m;

                }),
                Total = activityLog.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

    }
}
