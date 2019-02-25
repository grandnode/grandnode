using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Stores)]
    public partial class StoreController : BaseAdminController
    {
        private readonly IStoreViewModelService _storeViewModelService;
        private readonly IStoreService _storeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;

        public StoreController(
            IStoreViewModelService storeViewModelService,
            IStoreService storeService,
            ILanguageService languageService,
            ILocalizationService localizationService)
        {
            this._storeViewModelService = storeViewModelService;
            this._storeService = storeService;
            this._languageService = languageService;
            this._localizationService = localizationService;
        }

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var storeModels = _storeService.GetAllStores()
                .Select(x => x.ToModel())
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = storeModels,
                Total = storeModels.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            var model = _storeViewModelService.PrepareStoreModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //languages
            _storeViewModelService.PrepareLanguagesModel(model);
            //warehouses
            _storeViewModelService.PrepareWarehouseModel(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(StoreModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var store = _storeViewModelService.InsertStoreModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }
            //languages
            _storeViewModelService.PrepareLanguagesModel(model);
            //warehouses
            _storeViewModelService.PrepareWarehouseModel(model);

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var store = _storeService.GetStoreById(id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            var model = store.ToModel();
            //languages
            _storeViewModelService.PrepareLanguagesModel(model);
            //warehouses
            _storeViewModelService.PrepareWarehouseModel(model);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = store.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(StoreModel model, bool continueEditing)
        {
            var store = _storeService.GetStoreById(model.Id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                store = _storeViewModelService.UpdateStoreModel(store, model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //languages
            _storeViewModelService.PrepareLanguagesModel(model);
            //warehouses
            _storeViewModelService.PrepareWarehouseModel(model);

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var store = _storeService.GetStoreById(id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _storeViewModelService.DeleteStore(store);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = store.Id });
        }
    }
}
