using Grand.Core;
using Grand.Domain.Catalog;
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
using System.Threading.Tasks;
using Grand.Domain.Customers;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Manufacturers)]
    public partial class ManufacturerController : BaseAdminController
    {
        #region Fields
        private readonly IManufacturerViewModelService _manufacturerViewModelService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
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
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            IStoreService storeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IExportManager exportManager,
            IImportManager importManager)
        {
            _manufacturerViewModelService = manufacturerViewModelService;
            _manufacturerService = manufacturerService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _storeService = storeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _exportManager = exportManager;
            _importManager = importManager;
        }

        #endregion

        #region Utilities

        protected (bool allow, string message) CheckAccessToManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
            {
                return (false, "Manufacturer not exists");
            }
            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!(!manufacturer.LimitedToStores || (manufacturer.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && manufacturer.LimitedToStores)))
                    return (false, "This is not your manufacturer");
            }
            return (true, null);
        }

        #endregion

        #region List

        public IActionResult Index() => RedirectToAction("List");

        public async Task<IActionResult> List()
        {
            var storeId = _workContext.CurrentCustomer.StaffStoreId;
            var model = new ManufacturerListModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command, ManufacturerListModel model)
        {
            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;
            }
            var manufacturers = await _manufacturerService.GetAllManufacturers(model.SearchManufacturerName,
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

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create([FromServices] CatalogSettings catalogSettings)
        {
            var model = new ManufacturerModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            //templates
            await _manufacturerViewModelService.PrepareTemplatesModel(model);
            //discounts
            await _manufacturerViewModelService.PrepareDiscountModel(model, null, true);
            //ACL
            await model.PrepareACLModel(null, false, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);
            //default values
            model.PageSize = catalogSettings.DefaultManufacturerPageSize;
            model.PageSizeOptions = catalogSettings.DefaultManufacturerPageSizeOptions;
            model.Published = true;
            model.AllowCustomersToSelectPageSize = true;
            //sort options
            _manufacturerViewModelService.PrepareSortOptionsModel(model);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(ManufacturerModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }

                var manufacturer = await _manufacturerViewModelService.InsertManufacturerModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = manufacturer.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //templates
            await _manufacturerViewModelService.PrepareTemplatesModel(model);
            //discounts
            await _manufacturerViewModelService.PrepareDiscountModel(model, null, true);
            //ACL
            await model.PrepareACLModel(null, true, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);
            //sort options
            _manufacturerViewModelService.PrepareSortOptionsModel(model);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!manufacturer.LimitedToStores || (manufacturer.LimitedToStores && manufacturer.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && manufacturer.Stores.Count > 1))
                    WarningNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Permisions"));
                else
                {
                    if (!manufacturer.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                        return RedirectToAction("List");
                }
            }

            var model = manufacturer.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = manufacturer.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = manufacturer.GetLocalized(x => x.Description, languageId, false, false);
                locale.BottomDescription = manufacturer.GetLocalized(x => x.BottomDescription, languageId, false, false);
                locale.MetaKeywords = manufacturer.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = manufacturer.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = manufacturer.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = manufacturer.GetSeName(languageId, false, false);
            });
            //templates
            await _manufacturerViewModelService.PrepareTemplatesModel(model);
            //discounts
            await _manufacturerViewModelService.PrepareDiscountModel(model, manufacturer, false);
            //ACL
            await model.PrepareACLModel(manufacturer, false, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(manufacturer, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);
            //sort options
            _manufacturerViewModelService.PrepareSortOptionsModel(model);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(ManufacturerModel model, bool continueEditing)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(model.Id);
            if (manufacturer == null)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!manufacturer.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = manufacturer.Id });
            }
            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }
                manufacturer = await _manufacturerViewModelService.UpdateManufacturerModel(manufacturer, model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = manufacturer.Id });
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            //templates
            await _manufacturerViewModelService.PrepareTemplatesModel(model);
            //discounts
            await _manufacturerViewModelService.PrepareDiscountModel(model, manufacturer, true);
            //ACL
            await model.PrepareACLModel(manufacturer, true, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(manufacturer, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);
            //sort options
            _manufacturerViewModelService.PrepareSortOptionsModel(model);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!manufacturer.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = manufacturer.Id });
            }

            if (ModelState.IsValid)
            {
                await _manufacturerViewModelService.DeleteManufacturer(manufacturer);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = manufacturer.Id });
        }

        #endregion

        #region Export / Import

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        public async Task<IActionResult> ExportXml()
        {
            try
            {
                var manufacturers = await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: _workContext.CurrentCustomer.StaffStoreId);
                var xml = await _exportManager.ExportManufacturersToXml(manufacturers);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "manufacturers.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        public async Task<IActionResult> ExportXlsx()
        {
            try
            {
                var bytes = _exportManager.ExportManufacturersToXlsx(await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: _workContext.CurrentCustomer.StaffStoreId));
                return File(bytes, "text/xls", "manufacturers.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Import)]
        [HttpPost]
        public async Task<IActionResult> ImportFromXlsx(IFormFile importexcelfile, [FromServices] IWorkContext workContext)
        {
            //a vendor and staff cannot import manufacturers
            if (workContext.CurrentVendor != null || _workContext.CurrentCustomer.IsStaff())
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _importManager.ImportManufacturerFromXlsx(importexcelfile.OpenReadStream());
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

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ProductList(DataSourceRequest command, string manufacturerId)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(manufacturerId);
            var permission = CheckAccessToManufacturer(manufacturer);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var (manufacturerProductModels, totalCount) = await _manufacturerViewModelService.PrepareManufacturerProductModel(manufacturerId, _storeContext.CurrentStore.Id, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = manufacturerProductModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductUpdate(ManufacturerModel.ManufacturerProductModel model)
        {
            if (ModelState.IsValid)
            {
                await _manufacturerViewModelService.ProductUpdate(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductDelete(ManufacturerModel.ManufacturerProductModel model)
        {
            if (ModelState.IsValid)
            {
                await _manufacturerViewModelService.ProductDelete(model.Id, model.ProductId);
                return new NullJsonResult();
            }

            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAddPopup(string manufacturerId)
        {
            var model = await _manufacturerViewModelService.PrepareAddManufacturerProductModel(_workContext.CurrentCustomer.StaffStoreId);
            model.ManufacturerId = manufacturerId;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAddPopupList(DataSourceRequest command, ManufacturerModel.AddManufacturerProductModel model)
        {
            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;
            }
            var products = await _manufacturerViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = products.products.ToList(),
                Total = products.totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ProductAddPopup(ManufacturerModel.AddManufacturerProductModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedProductIds != null)
                {
                    await _manufacturerViewModelService.InsertManufacturerProductModel(model);
                }
                ViewBag.RefreshPage = true;
            }
            else
                ErrorNotification(ModelState);

            return View(model);
        }
        #endregion

        #region Activity log

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ListActivityLog(DataSourceRequest command, string manufacturerId)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(manufacturerId);
            var permission = CheckAccessToManufacturer(manufacturer);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var (activityLogModels, totalCount) = await _manufacturerViewModelService.PrepareActivityLogModel(manufacturerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLogModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }
        #endregion
    }

}
