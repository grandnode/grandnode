﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Localization;
using Grand.Core;
using Grand.Core.Domain.Localization;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Framework;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;
using Grand.Web.Framework.Security;

namespace Grand.Admin.Controllers
{
    public partial class LanguageController : BaseAdminController
	{
		#region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;

		#endregion

		#region Constructors

		public LanguageController(ILanguageService languageService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            IStoreService storeService, 
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            IWebHelper webHelper)
		{
			this._localizationService = localizationService;
            this._languageService = languageService;
            this._currencyService = currencyService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._webHelper= webHelper;
		}

		#endregion 

        #region Utilities

        [NonAction]
        protected virtual void PrepareFlagsModel(LanguageModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            
            model.FlagFileNames = Directory
                .EnumerateFiles(CommonHelper.MapPath("~/Content/Images/flags/"), "*.png", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .ToList();
        }

        [NonAction]
        protected virtual void PrepareStoresMappingModel(LanguageModel model, Language language, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (language != null)
                {
                    model.SelectedStoreIds = language.Stores.ToArray();
                }
            }
        }

        [NonAction]
        protected virtual void PrepareCurrenciesModel(LanguageModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //templates
            model.AvailableCurrencies.Add(new SelectListItem
                {
                    Text = "---",
                    Value = ""
                });
            var currencies = _currencyService.GetAllCurrencies(true);
            foreach (var currency in currencies)
            {
                model.AvailableCurrencies.Add(new SelectListItem
                {
                    Text = currency.Name,
                    Value = currency.Id.ToString()
                });
            }
        }

        #endregion

        #region Languages

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

			return View();
		}

		[HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

			var languages = _languageService.GetAllLanguages(true);
			var gridModel = new DataSourceResult
			{
				Data = languages.Select(x => x.ToModel()),
				Total = languages.Count()
			};
			return new JsonResult
			{
				Data = gridModel
			};
		}
        
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var model = new LanguageModel();
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //currencies
            PrepareCurrenciesModel(model);
            //flags
            PrepareFlagsModel(model);
            //default values
            model.Published = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(LanguageModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var language = model.ToEntity();
                language.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _languageService.InsertLanguage(language);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = language.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //Stores
            PrepareStoresMappingModel(model, null, true);
            //currencies
            PrepareCurrenciesModel(model);
            //flags
            PrepareFlagsModel(model);

            return View(model);
        }

		public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

			var language = _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

		    var model = language.ToModel();
            //Stores
            PrepareStoresMappingModel(model, language, false);
            //currencies
            PrepareCurrenciesModel(model);
            //flags
            PrepareFlagsModel(model);

            return View(model);
		}

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		public ActionResult Edit(LanguageModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

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

                //update
                language = model.ToEntity(language);
                language.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                _languageService.UpdateLanguage(language);

                //notification
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = language.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //Stores
            PrepareStoresMappingModel(model, language, true);
            //currencies
            PrepareCurrenciesModel(model);
            //flags
            PrepareFlagsModel(model);

            return View(model);
		}

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

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
            _languageService.DeleteLanguage(language);

            //notification
            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Languages.Deleted"));
            return RedirectToAction("List");
        }

		#endregion

		#region Resources

        [HttpPost]
        //do not validate request token (XSRF)
        //for some reasons it does not work with "filtering" support
        [AdminAntiForgery(true)] 
		public ActionResult Resources(string languageId, DataSourceRequest command,
            Grand.Web.Framework.Kendoui.Filter filter = null, IEnumerable<Sort> sort = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();
            
		    var language = _languageService.GetLanguageById(languageId);

            var resources = _localizationService
                .GetAllResourceValues(languageId)
                .OrderBy(x => x.Key)
                .Select(x => new LanguageResourceModel
                    {
                        LanguageId = languageId,
                        Id = x.Value.Key,
                        Name = x.Key,
                        Value = x.Value.Value,
                    })
                    .AsQueryable()
                    .Filter(filter)
                    .Sort(sort);
            
            var gridModel = new DataSourceResult
            {
                Data = resources.PagedForCommand(command),
                Total = resources.Count()
            };

            return Json(gridModel);
		}

        [HttpPost]
        public ActionResult ResourceUpdate(LanguageResourceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var resource = _localizationService.GetLocaleStringResourceById(model.Id);
            // if the resourceName changed, ensure it isn't being used by another resource
            if (!resource.ResourceName.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                var res = _localizationService.GetLocaleStringResourceByName(model.Name, model.LanguageId, false);
                if (res != null && res.Id != resource.Id)
                {
                    return Json(new DataSourceResult { Errors = string.Format(_localizationService.GetResource("Admin.Configuration.Languages.Resources.NameAlreadyExists"), res.ResourceName) });
                }
            }

            resource.ResourceName = model.Name;
            resource.ResourceValue = model.Value;
            _localizationService.UpdateLocaleStringResource(resource);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ResourceAdd(string languageId, [Bind(Exclude = "Id")] LanguageResourceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var res = _localizationService.GetLocaleStringResourceByName(model.Name, model.LanguageId, false);
            if (res == null)
            {
                var resource = new LocaleStringResource { LanguageId = languageId };
                resource.ResourceName = model.Name;
                resource.ResourceValue = model.Value;
                _localizationService.InsertLocaleStringResource(resource);
            }
            else
            {
                return Json(new DataSourceResult { Errors = string.Format(_localizationService.GetResource("Admin.Configuration.Languages.Resources.NameAlreadyExists"), model.Name) });
            }

            return new NullJsonResult();
        }
        
        [HttpPost]
        public ActionResult ResourceDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var resource = _localizationService.GetLocaleStringResourceById(id);
            if (resource == null)
                throw new ArgumentException("No resource found with the specified id");
            _localizationService.DeleteLocaleStringResource(resource);

            return new NullJsonResult();
        }

        #endregion
        
        #region Export / Import

        public ActionResult ExportXml(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var language = _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            try
            {
                var xml = _localizationService.ExportResourcesToXml(language);
                return new XmlDownloadResult(xml, "language_pack.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public ActionResult ImportXml(string id, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var language = _languageService.GetLanguageById(id);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

            try
            {
                var file = Request.Files["importxmlfile"];
                if (file != null && file.ContentLength > 0)
                {
                    using (var sr = new StreamReader(file.InputStream, Encoding.UTF8))
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
