using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Categories)]
    public partial class CategoryController : BaseAdminController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly ICategoryViewModelService _categoryViewModelService;
        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly IExportManager _exportManager;
        private readonly IWorkContext _workContext;
        private readonly IImportManager _importManager;

        #endregion

        #region Constructors

        public CategoryController(
            ICategoryService categoryService,
            ICategoryViewModelService categoryViewModelService,
            ICustomerService customerService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreService storeService,
            IExportManager exportManager,
            IWorkContext workContext,
            IImportManager importManager)
        {
            this._categoryService = categoryService;
            this._categoryViewModelService = categoryViewModelService;
            this._customerService = customerService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._storeService = storeService;
            this._exportManager = exportManager;
            this._workContext = workContext;
            this._importManager = importManager;
        }

        #endregion

        #region List / tree

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = _categoryViewModelService.PrepareCategoryListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, CategoryListModel model)
        {
            var categories = _categoryViewModelService.PrepareCategoryListModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = categories.categoryListModel,
                Total = categories.totalCount
            };
            return Json(gridModel);
        }

        public IActionResult Tree() => View();

        public IActionResult NodeList()
        {
            var model = _categoryViewModelService.PrepareCategoryNodeListModel();
            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete

        public IActionResult Create()
        {
            var model = _categoryViewModelService.PrepareCategoryModel();
            //locales
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CategoryModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var category = _categoryViewModelService.InsertCategoryModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = category.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model = _categoryViewModelService.PrepareCategoryModel(model, null);
            //ACL
            model.PrepareACLModel(null, true, _customerService);
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);

            return View(model);
        }

        public IActionResult Edit(string id)
        {
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
            model = _categoryViewModelService.PrepareCategoryModel(model, category);
            //ACL
            model.PrepareACLModel(category, false, _customerService);
            //Stores
            model.PrepareStoresMappingModel(category, false, _storeService);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CategoryModel model, bool continueEditing)
        {
            var category = _categoryService.GetCategoryById(model.Id);
            if (category == null)
                //No category found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                category = _categoryViewModelService.UpdateCategoryModel(category, model);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = category.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model = _categoryViewModelService.PrepareCategoryModel(model, category);
            //ACL
            model.PrepareACLModel(category, true, _customerService);
            //Stores
            model.PrepareStoresMappingModel(category, true, _storeService);

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var category = _categoryService.GetCategoryById(id);
            if (category == null)
                //No category found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _categoryViewModelService.DeleteCategory(category);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Deleted"));
            }
            return RedirectToAction("List");
        }


        #endregion

        #region Export / Import

        public IActionResult ExportXml()
        {
            try
            {
                var xml = _exportManager.ExportCategoriesToXml();
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "categories.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public IActionResult ExportXlsx()
        {
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
        public IActionResult ImportFromXlsx(IFormFile importexcelfile)
        {
            //a vendor cannot import categories
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportCategoryFromXlsx(importexcelfile.OpenReadStream());
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
        public IActionResult ProductList(DataSourceRequest command, string categoryId)
        {
            var productCategories = _categoryViewModelService.PrepareCategoryProductModel(categoryId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = productCategories.categoryProductModels,
                Total = productCategories.totalCount
            };

            return Json(gridModel);
        }

        public IActionResult ProductUpdate(CategoryModel.CategoryProductModel model)
        {
            if (ModelState.IsValid)
            {
                _categoryViewModelService.UpdateProductCategoryModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult ProductDelete(string id, string productId)
        {
            if (ModelState.IsValid)
            {
                _categoryViewModelService.DeleteProductCategoryModel(id, productId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult ProductAddPopup(string categoryId)
        {
            var model = _categoryViewModelService.PrepareAddCategoryProductModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, CategoryModel.AddCategoryProductModel model)
        {
            var gridModel = new DataSourceResult();
            var products = _categoryViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            gridModel.Data = products.products.ToList();
            gridModel.Total = products.totalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(CategoryModel.AddCategoryProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _categoryViewModelService.InsertCategoryProductModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Activity log

        [HttpPost]
        public IActionResult ListActivityLog(DataSourceRequest command, string categoryId)
        {
            var activityLog = _categoryViewModelService.PrepareActivityLogModel(categoryId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.activityLogModel,
                Total = activityLog.totalCount
            };

            return Json(gridModel);
        }

        #endregion
    }
}
