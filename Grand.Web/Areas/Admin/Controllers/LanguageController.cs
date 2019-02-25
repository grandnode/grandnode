using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
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
            this._languageViewModelService = languageViewModelService;
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._storeService = storeService;
        }

        #endregion


        #region Languages

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var languages = _languageService.GetAllLanguages(true);
            var gridModel = new DataSourceResult
            {
                Data = languages.Select(x => x.ToModel()),
                Total = languages.Count()
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            var model = new LanguageModel();
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);
            //currencies
            _languageViewModelService.PrepareCurrenciesModel(model);
            //flags
            _languageViewModelService.PrepareFlagsModel(model);
            //default values
            model.Published = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(LanguageModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var language = _languageViewModelService.InsertLanguageModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = language.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);
            //currencies
            _languageViewModelService.PrepareCurrenciesModel(model);
            //flags
            _languageViewModelService.PrepareFlagsModel(model);

            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var language = _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            var model = language.ToModel();
            //Stores
            model.PrepareStoresMappingModel(language, false, _storeService);
            //currencies
            _languageViewModelService.PrepareCurrenciesModel(model);
            //flags
            _languageViewModelService.PrepareFlagsModel(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(LanguageModel model, bool continueEditing)
        {
            var language = _languageService.GetLanguageById(model.Id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //ensure we have at least one published language
                var allLanguages = _languageService.GetAllLanguages();
                if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id &&
                    !model.Published)
                {
                    ErrorNotification("At least one published language is required.");
                    return RedirectToAction("Edit", new { id = language.Id });
                }

                language = _languageViewModelService.UpdateLanguageModel(language, model);
                //notification
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = language.Id });
                }
                return RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            //Stores
            model.PrepareStoresMappingModel(language, true, _storeService);
            //currencies
            _languageViewModelService.PrepareCurrenciesModel(model);
            //flags
            _languageViewModelService.PrepareFlagsModel(model);

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var language = _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            //ensure we have at least one published language
            var allLanguages = _languageService.GetAllLanguages();
            if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id)
            {
                ErrorNotification("At least one published language is required.");
                return RedirectToAction("Edit", new { id = language.Id });
            }

            //delete
            if (ModelState.IsValid)
            {
                _languageService.DeleteLanguage(language);

                //notification
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = language.Id });
        }

        #endregion

        #region Resources

        [HttpPost]
        [AdminAntiForgery(true)]
        public IActionResult Resources(string languageId, DataSourceRequest command,
            LanguageResourceFilterModel model)
        {
            var resources = _languageViewModelService.PrepareLanguageResourceModel(model, languageId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = resources.languageResourceModels.ToList(),
                Total = resources.totalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ResourceUpdate(LanguageResourceModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var result = _languageViewModelService.UpdateLanguageResourceModel(model);
            if (result.error)
                return ErrorForKendoGridJson(result.message);
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ResourceAdd(LanguageResourceModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            var result = _languageViewModelService.InsertLanguageResourceModel(model);
            if (result.error)
            {
                return ErrorForKendoGridJson(result.message);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ResourceDelete(string id)
        {
            var resource = _localizationService.GetLocaleStringResourceById(id);
            if (resource == null)
                throw new ArgumentException("No resource found with the specified id");
            if (ModelState.IsValid)
            {
                _localizationService.DeleteLocaleStringResource(resource);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Export / Import

        public IActionResult ExportXml(string id)
        {
            var language = _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            try
            {
                var xml = _localizationService.ExportResourcesToXml(language);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "language_pack.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ImportXml(string id, IFormFile importxmlfile)
        {
            var language = _languageService.GetLanguageById(id);
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
                        _localizationService.ImportResourcesFromXml(language, content);
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