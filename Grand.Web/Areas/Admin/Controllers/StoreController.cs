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
using System.Threading.Tasks;

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
            _storeViewModelService = storeViewModelService;
            _storeService = storeService;
            _languageService = languageService;
            _localizationService = localizationService;
        }

        public IActionResult List() => View();

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var storeModels = (await _storeService.GetAllStores())
                .Select(x => x.ToModel())
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = storeModels,
                Total = storeModels.Count()
            };

            return Json(gridModel);
        }

        public async Task<IActionResult> Create()
        {
            var model = _storeViewModelService.PrepareStoreModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            //languages
            await _storeViewModelService.PrepareLanguagesModel(model);
            //warehouses
            await _storeViewModelService.PrepareWarehouseModel(model);
            //countries
            await _storeViewModelService.PrepareCountryModel(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(StoreModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var store = await _storeViewModelService.InsertStoreModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }
            //languages
            await _storeViewModelService.PrepareLanguagesModel(model);
            //warehouses
            await _storeViewModelService.PrepareWarehouseModel(model);
            //countries
            await _storeViewModelService.PrepareCountryModel(model);

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var store = await _storeService.GetStoreById(id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            var model = store.ToModel();
            //languages
            await _storeViewModelService.PrepareLanguagesModel(model);
            //warehouses
            await _storeViewModelService.PrepareWarehouseModel(model);
            //countries
            await _storeViewModelService.PrepareCountryModel(model);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = store.GetLocalized(x => x.Name, languageId, false, false);
                locale.Shortcut = store.GetLocalized(x => x.Shortcut, languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(StoreModel model, bool continueEditing)
        {
            var store = await _storeService.GetStoreById(model.Id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                store = await _storeViewModelService.UpdateStoreModel(store, model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //languages
            await _storeViewModelService.PrepareLanguagesModel(model);
            //warehouses
            await _storeViewModelService.PrepareWarehouseModel(model);
            //countries
            await _storeViewModelService.PrepareCountryModel(model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var store = await _storeService.GetStoreById(id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                await _storeViewModelService.DeleteStore(store);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = store.Id });
        }
    }
}
