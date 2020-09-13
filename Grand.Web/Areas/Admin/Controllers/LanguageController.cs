using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Languages)]
    public partial class LanguageController : BaseAdminController
    {
        #region Fields
        private readonly ILanguageViewModelService _languageViewModelService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        #endregion

        #region Constructors

        public LanguageController(
            ILanguageViewModelService languageViewModelService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreService storeService)
        {
            _languageViewModelService = languageViewModelService;
            _localizationService = localizationService;
            _languageService = languageService;
            _storeService = storeService;
        }

        #endregion


        #region Languages

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var languages = await _languageService.GetAllLanguages(true);
            var gridModel = new DataSourceResult {
                Data = languages.Select(x => x.ToModel()),
                Total = languages.Count()
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = new LanguageModel();
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false);
            //currencies
            await _languageViewModelService.PrepareCurrenciesModel(model);
            //flags
            _languageViewModelService.PrepareFlagsModel(model);
            //default values
            model.Published = true;
            model.DisplayOrder = (await _languageService.GetAllLanguages()).Max(x => x.DisplayOrder) + 1;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(LanguageModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var language = await _languageViewModelService.InsertLanguageModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = language.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true);
            //currencies
            await _languageViewModelService.PrepareCurrenciesModel(model);
            //flags
            _languageViewModelService.PrepareFlagsModel(model);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var language = await _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            var model = language.ToModel();
            //Stores
            await model.PrepareStoresMappingModel(language, _storeService, false);
            //currencies
            await _languageViewModelService.PrepareCurrenciesModel(model);
            //flags
            _languageViewModelService.PrepareFlagsModel(model);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(LanguageModel model, bool continueEditing)
        {
            var language = await _languageService.GetLanguageById(model.Id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //ensure we have at least one published language
                var allLanguages = await _languageService.GetAllLanguages();
                if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id &&
                    !model.Published)
                {
                    ErrorNotification("At least one published language is required.");
                    return RedirectToAction("Edit", new { id = language.Id });
                }

                language = await _languageViewModelService.UpdateLanguageModel(language, model);
                //notification
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = language.Id });
                }
                return RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            //Stores
            await model.PrepareStoresMappingModel(language, _storeService, true);
            //currencies
            await _languageViewModelService.PrepareCurrenciesModel(model);
            //flags
            _languageViewModelService.PrepareFlagsModel(model);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var language = await _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            //ensure we have at least one published language
            var allLanguages = await _languageService.GetAllLanguages();
            if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id)
            {
                ErrorNotification("At least one published language is required.");
                return RedirectToAction("Edit", new { id = language.Id });
            }

            //delete
            if (ModelState.IsValid)
            {
                await _languageService.DeleteLanguage(language);

                //notification
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = language.Id });
        }

        #endregion

        #region Resources

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Resources(string languageId, DataSourceRequest command,
            LanguageResourceFilterModel model)
        {
            var (languageResourceModels, totalCount) = await _languageViewModelService.PrepareLanguageResourceModel(model, languageId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = languageResourceModels.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ResourceUpdate(LanguageResourceModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var (error, message) = await _languageViewModelService.UpdateLanguageResourceModel(model);
            if (error)
                return ErrorForKendoGridJson(message);
            return new NullJsonResult();
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ResourceAdd(LanguageResourceModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            var (error, message) = await _languageViewModelService.InsertLanguageResourceModel(model);
            if (error)
            {
                return ErrorForKendoGridJson(message);
            }
            return new NullJsonResult();
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ResourceDelete(string id)
        {
            var resource = await _localizationService.GetLocaleStringResourceById(id);
            if (resource == null)
                throw new ArgumentException("No resource found with the specified id");
            if (ModelState.IsValid)
            {
                await _localizationService.DeleteLocaleStringResource(resource);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Export / Import
        [PermissionAuthorizeAction(PermissionActionName.Export)]
        public async Task<IActionResult> ExportXml(string id)
        {
            var language = await _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            try
            {
                var xml = await _localizationService.ExportResourcesToXml(language);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "language_pack.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Import)]
        [HttpPost]
        public async Task<IActionResult> ImportXml(string id, IFormFile importxmlfile)
        {
            var language = await _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            try
            {
                if (importxmlfile != null && importxmlfile.Length > 0)
                {
                    using (var sr = new StreamReader(importxmlfile.OpenReadStream(), Encoding.UTF8))
                    {
                        string content = sr.ReadToEnd();
                        await _localizationService.ImportResourcesFromXml(language, content);
                    }
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("Edit", new { id = language.Id });
                }

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Imported"));
                return RedirectToAction("Edit", new { id = language.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = language.Id });
            }
        }

        #endregion
    }
}