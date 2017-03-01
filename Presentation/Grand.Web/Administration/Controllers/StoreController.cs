using System;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Stores;
using Grand.Core.Domain.Stores;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using System.Collections.Generic;
using Grand.Core.Domain.Localization;
using Grand.Services.Shipping;

namespace Grand.Admin.Controllers
{
    public partial class StoreController : BaseAdminController
    {
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IShippingService _shippingService;

        public StoreController(IStoreService storeService,
            ISettingService settingService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IShippingService shippingService)
        {
            this._storeService = storeService;
            this._settingService = settingService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._shippingService = shippingService;
        }

        [NonAction]
        protected virtual void PrepareLanguagesModel(StoreModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //templates
            model.AvailableLanguages.Add(new SelectListItem
            {
                Text = "---",
                Value = ""
            });
            var languages = _languageService.GetAllLanguages(true);
            foreach (var language in languages)
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Text = language.Name,
                    Value = language.Id.ToString()
                });
            }
        }
        [NonAction]
        protected virtual void PrepareWarehouseModel(StoreModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //templates
            model.AvailableWarehouses.Add(new SelectListItem
            {
                Text = "---",
                Value = ""
            });
            var warehouses = _shippingService.GetAllWarehouses();
            foreach (var warehouse in warehouses)
            {
                model.AvailableWarehouses.Add(new SelectListItem
                {
                    Text = warehouse.Name,
                    Value = warehouse.Id.ToString()
                });
            }
        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateAttributeLocales(Store store, StoreModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {
                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "Name",
                    LocaleValue = local.Name
                });
            }
            return localized;
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

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

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var model = new StoreModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //languages
            PrepareLanguagesModel(model);
            //warehouses
            PrepareWarehouseModel(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(StoreModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();
            
            if (ModelState.IsValid)
            {
                var store = model.ToEntity();
                //ensure we have "/" at the end
                if (!store.Url.EndsWith("/"))
                    store.Url += "/";
                store.Locales = UpdateAttributeLocales(store, model);
                _storeService.InsertStore(store);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }
            //languages
            PrepareLanguagesModel(model);
            //warehouses
            PrepareWarehouseModel(model);

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = _storeService.GetStoreById(id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            var model = store.ToModel();
            //languages
            PrepareLanguagesModel(model);
            //warehouses
            PrepareWarehouseModel(model);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = store.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(StoreModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = _storeService.GetStoreById(model.Id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                store = model.ToEntity(store);
                //ensure we have "/" at the end
                if (!store.Url.EndsWith("/"))
                    store.Url += "/";

                store.Locales = UpdateAttributeLocales(store, model);
                _storeService.UpdateStore(store);
                //locales

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //languages
            PrepareLanguagesModel(model);
            //warehouses
            PrepareWarehouseModel(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = _storeService.GetStoreById(id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            try
            {
                _storeService.DeleteStore(store);

                //when we delete a store we should also ensure that all "per store" settings will also be deleted
                var settingsToDelete = _settingService
                    .GetAllSettings()
                    .Where(s => s.StoreId == id)
                    .ToList();
                foreach (var setting in settingsToDelete)
                    _settingService.DeleteSetting(setting);
                //when we had two stores and now have only one store, we also should delete all "per store" settings
                var allStores = _storeService.GetAllStores();
                if (allStores.Count == 1)
                {
                    settingsToDelete = _settingService
                        .GetAllSettings()
                        .Where(s => s.StoreId == allStores[0].Id)
                        .ToList();
                    foreach (var setting in settingsToDelete)
                        _settingService.DeleteSetting(setting);
                }


                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new {id = store.Id});
            }
        }
    }
}
