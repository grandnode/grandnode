using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Localization;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Discounts;
using Grand.Services.ExportImport;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ManufacturerController : BaseAdminController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IExportManager _exportManager;
        private readonly IDiscountService _discountService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IVendorService _vendorService;
        private readonly IAclService _aclService; 
        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly IImportManager _importManager;
        private readonly CatalogSettings _catalogSettings;

        #endregion
        
        #region Constructors

        public ManufacturerController(ICategoryService categoryService, 
            IManufacturerService manufacturerService,
            IManufacturerTemplateService manufacturerTemplateService,
            IProductService productService,
            ICustomerService customerService, 
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService, 
            IPictureService pictureService,
            ILanguageService languageService, 
            ILocalizationService localizationService,
            IExportManager exportManager,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService, 
            IVendorService vendorService,
            IAclService aclService,
            IPermissionService permissionService,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            IImportManager importManager,
            CatalogSettings catalogSettings)
        {
            this._categoryService = categoryService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._customerService = customerService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._exportManager = exportManager;
            this._discountService = discountService;
            this._customerActivityService = customerActivityService;
            this._vendorService = vendorService;
            this._aclService = aclService;
            this._permissionService = permissionService;
            this._dateTimeHelper = dateTimeHelper;
            this._workContext = workContext;
            this._importManager = importManager;
            this._catalogSettings = catalogSettings;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(Manufacturer manufacturer, ManufacturerModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();

            foreach (var local in model.Locales)
            {
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
               
                //search engine name
                var seName = manufacturer.ValidateSeName(local.SeName, local.Name, false);
                _urlRecordService.SaveSlug(manufacturer, seName, local.LanguageId);

                if (!(String.IsNullOrEmpty(seName)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "SeName",
                        LocaleValue = seName,
                    });
            }
            return localized;
        }

        [NonAction]
        protected virtual void UpdatePictureSeoNames(Manufacturer manufacturer)
        {
            var picture = _pictureService.GetPictureById(manufacturer.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(manufacturer.Name));
        }

        [NonAction]
        protected virtual void PrepareTemplatesModel(ManufacturerModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = _manufacturerTemplateService.GetAllManufacturerTemplates();
            foreach (var template in templates)
            {
                model.AvailableManufacturerTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }

        [NonAction]
        protected virtual void PrepareDiscountModel(ManufacturerModel model, Manufacturer manufacturer, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableDiscounts = _discountService
                .GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true)
                .Select(d => d.ToModel())
                .ToList();

            if (!excludeProperties && manufacturer != null)
            {
                model.SelectedDiscountIds = manufacturer.AppliedDiscounts.ToArray();
            }
        }

        [NonAction]
        protected virtual void PrepareAclModel(ManufacturerModel model, Manufacturer manufacturer, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (manufacturer != null)
                {
                    model.SelectedCustomerRoleIds = manufacturer.CustomerRoles.ToArray();
                }
            }
        }

        

        [NonAction]
        protected virtual void PrepareStoresMappingModel(ManufacturerModel model, Manufacturer manufacturer, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (manufacturer != null)
                {
                    model.SelectedStoreIds = manufacturer.Stores.ToArray();
                }
            }
        }

        
        #endregion
        
        #region List

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var model = new ManufacturerListModel();            
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, ManufacturerListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturers = _manufacturerService.GetAllManufacturers(model.SearchManufacturerName, 
                model.SearchStoreId, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = manufacturers.Select(x => x.ToModel()),
                Total = manufacturers.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Create / Edit / Delete

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var model = new ManufacturerModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //templates
            PrepareTemplatesModel(model);
            //discounts
            PrepareDiscountModel(model, null, true);
            //ACL
            PrepareAclModel(model, null, false);
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //default values
            model.PageSize = _catalogSettings.DefaultManufacturerPageSize;
            model.PageSizeOptions = _catalogSettings.DefaultManufacturerPageSizeOptions;
            model.Published = true;
            model.AllowCustomersToSelectPageSize = true;
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(ManufacturerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var manufacturer = model.ToEntity();
                manufacturer.CreatedOnUtc = DateTime.UtcNow;
                manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                
                manufacturer.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                manufacturer.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        manufacturer.AppliedDiscounts.Add(discount.Id);
                }

                _manufacturerService.InsertManufacturer(manufacturer);
                //search engine name
                manufacturer.Locales = UpdateLocales(manufacturer, model);
                model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
                manufacturer.SeName = model.SeName;
                _manufacturerService.UpdateManufacturer(manufacturer);

                _urlRecordService.SaveSlug(manufacturer, model.SeName, "");
               
                //update picture seo file name
                UpdatePictureSeoNames(manufacturer);
                
                //activity log
                _customerActivityService.InsertActivity("AddNewManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = manufacturer.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
            //discounts
            PrepareDiscountModel(model, null, true);
            //ACL
            PrepareAclModel(model, null, true);
            //Stores
            PrepareStoresMappingModel(model, null, true);

            return View(model);
        }

        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            var model = manufacturer.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = manufacturer.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = manufacturer.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = manufacturer.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = manufacturer.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = manufacturer.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = manufacturer.GetSeName(languageId, false, false);
            });
            //templates
            PrepareTemplatesModel(model);
            //discounts
            PrepareDiscountModel(model, manufacturer, false);
            //ACL
            PrepareAclModel(model, manufacturer, false);
            //Stores
            PrepareStoresMappingModel(model, manufacturer, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(ManufacturerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(model.Id);
            if (manufacturer == null)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                string prevPictureId = manufacturer.PictureId;
                manufacturer = model.ToEntity(manufacturer);
                manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                manufacturer.Locales = UpdateLocales(manufacturer, model);
                manufacturer.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                manufacturer.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new discount
                        if (manufacturer.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                            manufacturer.AppliedDiscounts.Add(discount.Id);
                    }
                    else
                    {
                        //remove discount
                        if (manufacturer.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                            manufacturer.AppliedDiscounts.Remove(discount.Id);
                    }
                }
                model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
                manufacturer.SeName = model.SeName;

                _manufacturerService.UpdateManufacturer(manufacturer);
                //search engine name
                _urlRecordService.SaveSlug(manufacturer, model.SeName, "");
                
                //delete an old picture (if deleted or updated)
                if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != manufacturer.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }
                //update picture seo file name
                UpdatePictureSeoNames(manufacturer);
               
                //activity log
                _customerActivityService.InsertActivity("EditManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.EditManufacturer"), manufacturer.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit",  new {id = manufacturer.Id});
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
            //discounts
            PrepareDiscountModel(model, manufacturer, true);
            //ACL
            PrepareAclModel(model, manufacturer, true);
            //Stores
            PrepareStoresMappingModel(model, manufacturer, true);

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            _manufacturerService.DeleteManufacturer(manufacturer);

            //activity log
            _customerActivityService.InsertActivity("DeleteManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.DeleteManufacturer"), manufacturer.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Deleted"));
            return RedirectToAction("List");
        }
        
        #endregion

        #region Export / Import

        public IActionResult ExportXml()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            try
            {
                var manufacturers = _manufacturerService.GetAllManufacturers(showHidden: true);
                var xml = _exportManager.ExportManufacturersToXml(manufacturers);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "manufacturers.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


        public IActionResult ExportXlsx()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();
            try
            {
                var bytes = _exportManager.ExportManufacturersToXlsx(_manufacturerService.GetAllManufacturers(showHidden: true));
                return File(bytes, "text/xls", "manufacturers.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ImportFromXlsx(IFormFile importexcelfile)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            //a vendor cannot import manufacturers
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();
            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportManufacturerFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturer.Imported"));
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
        public IActionResult ProductList(DataSourceRequest command, string manufacturerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId,
                command.Page - 1, command.PageSize, true);

            var gridModel = new DataSourceResult
            {
                Data = productManufacturers
                .Select(x => new ManufacturerModel.ManufacturerProductModel
                {
                    Id = x.Id,
                    ManufacturerId = x.ManufacturerId,
                    ProductId = x.ProductId,
                    ProductName = _productService.GetProductById(x.ProductId).Name,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                }),
                Total = productManufacturers.TotalCount
            };


            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductUpdate(ManufacturerModel.ManufacturerProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productManufacturer = product.ProductManufacturers.Where(x=>x.Id == model.Id).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
            productManufacturer.DisplayOrder = model.DisplayOrder;
            productManufacturer.ProductId = model.ProductId;
            _manufacturerService.UpdateProductManufacturer(productManufacturer);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductDelete(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productManufacturer = product.ProductManufacturers.Where(x => x.Id == id).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.ProductId = productId;

            _manufacturerService.DeleteProductManufacturer(productManufacturer);

            return new NullJsonResult();
        }

        public IActionResult ProductAddPopup(string manufacturerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var model = new ManufacturerModel.AddManufacturerProductModel();
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

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, ManufacturerModel.AddManufacturerProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
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
        public IActionResult ProductAddPopup(ManufacturerModel.AddManufacturerProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (string id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var existingProductmanufacturers = product.ProductManufacturers;
                        if (product.ProductManufacturers.Where(x => x.ManufacturerId == model.ManufacturerId).Count() == 0)
                        {
                            _manufacturerService.InsertProductManufacturer(
                                new ProductManufacturer
                                {
                                    ManufacturerId = model.ManufacturerId,
                                    ProductId = id,
                                    IsFeaturedProduct = false,
                                    DisplayOrder = 1,
                                });
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }
        #endregion

        #region Activity log

        [HttpPost]
        public IActionResult ListActivityLog(DataSourceRequest command, string manufacturerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return Content("");

            var activityLog = _customerActivityService.GetManufacturerActivities(null, null, manufacturerId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
                {
                    var customer = _customerService.GetCustomerById(x.CustomerId);
                    var m = new ManufacturerModel.ActivityLogModel
                    {
                        Id = x.Id,
                        ActivityLogTypeName = _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId)?.Name,
                        Comment = x.Comment,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                        CustomerId = x.CustomerId,
                        CustomerEmail = customer != null ? customer.Email : "null"
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
