using Grand.Core;
using Grand.Core.Domain.Catalog;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Text;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Manufacturers)]
    public partial class ManufacturerController : BaseAdminController
    {
        #region Fields
        private readonly IManufacturerViewModelService _manufacturerViewModelService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        #endregion

        #region Constructors

        public ManufacturerController(
            IManufacturerViewModelService manufacturerViewModelService,
            IManufacturerService manufacturerService,
            ICustomerService customerService,
            IStoreService storeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IExportManager exportManager,
            IImportManager importManager)
        {
            this._manufacturerViewModelService = manufacturerViewModelService;
            this._manufacturerService = manufacturerService;
            this._customerService = customerService;
            this._storeService = storeService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._exportManager = exportManager;
            this._importManager = importManager;
        }

        #endregion

        #region List

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = new ManufacturerListModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, ManufacturerListModel model)
        {
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

        public IActionResult Create([FromServices] CatalogSettings catalogSettings)
        {
            var model = new ManufacturerModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //templates
            _manufacturerViewModelService.PrepareTemplatesModel(model);
            //discounts
            _manufacturerViewModelService.PrepareDiscountModel(model, null, true);
            //ACL
            model.PrepareACLModel(null, false, _customerService);
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);
            //default values
            model.PageSize = catalogSettings.DefaultManufacturerPageSize;
            model.PageSizeOptions = catalogSettings.DefaultManufacturerPageSizeOptions;
            model.Published = true;
            model.AllowCustomersToSelectPageSize = true;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(ManufacturerModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var manufacturer = _manufacturerViewModelService.InsertManufacturerModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = manufacturer.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //templates
            _manufacturerViewModelService.PrepareTemplatesModel(model);
            //discounts
            _manufacturerViewModelService.PrepareDiscountModel(model, null, true);
            //ACL
            model.PrepareACLModel(null, true, _customerService);
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);

            return View(model);
        }

        public IActionResult Edit(string id)
        {
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
            _manufacturerViewModelService.PrepareTemplatesModel(model);
            //discounts
            _manufacturerViewModelService.PrepareDiscountModel(model, manufacturer, false);
            //ACL
            model.PrepareACLModel(manufacturer, false, _customerService);
            //Stores
            model.PrepareStoresMappingModel(manufacturer, false, _storeService);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(ManufacturerModel model, bool continueEditing)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(model.Id);
            if (manufacturer == null)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                manufacturer = _manufacturerViewModelService.UpdateManufacturerModel(manufacturer, model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = manufacturer.Id });
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            //templates
            _manufacturerViewModelService.PrepareTemplatesModel(model);
            //discounts
            _manufacturerViewModelService.PrepareDiscountModel(model, manufacturer, true);
            //ACL
            model.PrepareACLModel(manufacturer, true, _customerService);
            //Stores
            model.PrepareStoresMappingModel(manufacturer, true, _storeService);

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _manufacturerViewModelService.DeleteManufacturer(manufacturer);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = manufacturer.Id });
        }

        #endregion

        #region Export / Import

        public IActionResult ExportXml()
        {
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
        public IActionResult ImportFromXlsx(IFormFile importexcelfile, [FromServices] IWorkContext workContext)
        {
            //a vendor cannot import manufacturers
            if (workContext.CurrentVendor != null)
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
            var productManufacturers = _manufacturerViewModelService.PrepareManufacturerProductModel(manufacturerId, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = productManufacturers.manufacturerProductModels.ToList(),
                Total = productManufacturers.totalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductUpdate(ManufacturerModel.ManufacturerProductModel model)
        {
            if (ModelState.IsValid)
            {
                _manufacturerViewModelService.ProductUpdate(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ProductDelete(string id, string productId)
        {
            if (ModelState.IsValid)
            {
                _manufacturerViewModelService.ProductDelete(id, productId);
                return new NullJsonResult();
            }

            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult ProductAddPopup(string manufacturerId)
        {
            var model = _manufacturerViewModelService.PrepareAddManufacturerProductModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, ManufacturerModel.AddManufacturerProductModel model)
        {
            var products = _manufacturerViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = products.products.ToList();
            gridModel.Total = products.totalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(ManufacturerModel.AddManufacturerProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _manufacturerViewModelService.InsertManufacturerProductModel(model);
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }
        #endregion

        #region Activity log

        [HttpPost]
        public IActionResult ListActivityLog(DataSourceRequest command, string manufacturerId)
        {
            var activityLog = _manufacturerViewModelService.PrepareActivityLogModel(manufacturerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.activityLogModels.ToList(),
                Total = activityLog.totalCount
            };
            return Json(gridModel);
        }

        #endregion


    }

}
