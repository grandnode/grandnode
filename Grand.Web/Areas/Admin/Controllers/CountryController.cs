using Grand.Core;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grand.Web.Areas.Admin.Models.Country;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Countries)]
    public partial class CountryController : BaseAdminController
	{
		#region Fields

        private readonly ICountryService _countryService;
        private readonly ICountryViewModelService _countryViewModelService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
	    private readonly IAddressService _addressService;
        private readonly ILanguageService _languageService;
        private readonly IStoreService _storeService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;

	    #endregion

		#region Constructors

        public CountryController(ICountryService countryService,
            ICountryViewModelService countryViewModelService,
            IStateProvinceService stateProvinceService, 
            ILocalizationService localizationService,
            IAddressService addressService, 
            ILanguageService languageService,
            IStoreService storeService,
            IExportManager exportManager,
            IImportManager importManager)
		{
            _countryService = countryService;
            _countryViewModelService = countryViewModelService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
            _addressService = addressService;
            _languageService = languageService;
            _storeService = storeService;
            _exportManager = exportManager;
            _importManager = importManager;
		}

		#endregion 

        #region Countries

        public IActionResult Index() => RedirectToAction("List");
        
        public IActionResult List()
        {
            var model = new CountriesListModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> CountryList(DataSourceRequest command, CountriesListModel countriesListModel)
        {
            var countries = await _countryService.GetAllCountries(showHidden: true);
            
            //Filters Countries based off of name
            if(!string.IsNullOrEmpty(countriesListModel.CountryName))
            {
                countries = countries.Where(
                    x => x.Name.ToLowerInvariant().Contains(countriesListModel.CountryName.ToLowerInvariant())
                    ).ToList();
            }

            var gridModel = new DataSourceResult
            {
                Data = countries.Select(x => x.ToModel()),
                Total = countries.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = _countryViewModelService.PrepareCountryModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false);
            
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(CountryModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var country = await _countryViewModelService.InsertCountryModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Countries.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = country.Id }) : RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var country = await _countryService.GetCountryById(id);
            if (country == null)
                //No country found with the specified id
                return RedirectToAction("List");

            var model = country.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = country.GetLocalized(x => x.Name, languageId, false, false);
            });
            //Stores
            await model.PrepareStoresMappingModel(country, _storeService, false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(CountryModel model, bool continueEditing)
        {
            var country = await _countryService.GetCountryById(model.Id);
            if (country == null)
                //No country found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                country = await _countryViewModelService.UpdateCountryModel(country, model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Countries.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = country.Id});
                }
                return RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            //Stores
            await model.PrepareStoresMappingModel(country, _storeService, true);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var country = await _countryService.GetCountryById(id);
            if (country == null)
                //No country found with the specified id
                return RedirectToAction("List");

            try
            {
                if (await _addressService.GetAddressTotalByCountryId(country.Id) > 0)
                    throw new GrandException("The country can't be deleted. It has associated addresses");
                if (ModelState.IsValid)
                {
                    await _countryService.DeleteCountry(country);
                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.Countries.Deleted"));
                    return RedirectToAction("List");
                }
                ErrorNotification(ModelState);
                return RedirectToAction("Edit", new { id = id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = country.Id });
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> PublishSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                var countries = await _countryService.GetCountriesByIds(selectedIds.ToArray());
                foreach (var country in countries)
                {
                    country.Published = true;
                    await _countryService.UpdateCountry(country);
                }
            }

            return Json(new { Result = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> UnpublishSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                var countries = await _countryService.GetCountriesByIds(selectedIds.ToArray());
                foreach (var country in countries)
                {
                    country.Published = false;
                    await _countryService.UpdateCountry(country);
                }
            }
            return Json(new { Result = true });
        }

        #endregion

        #region States / provinces

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> States(string countryId, DataSourceRequest command)
        {
            var states = await _stateProvinceService.GetStateProvincesByCountryId(countryId, showHidden: true);

            var gridModel = new DataSourceResult
            {
                Data = states.Select(x => x.ToModel()),
                Total = states.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        //create
        public async Task<IActionResult> StateCreatePopup(string countryId)
        {
            var model = _countryViewModelService.PrepareStateProvinceModel(countryId);
            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> StateCreatePopup(StateProvinceModel model)
        {
            var country = await _countryService.GetCountryById(model.CountryId);
            if (country == null)
                //No country found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                await _countryViewModelService.InsertStateProvinceModel(model);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        //edit
        public async Task<IActionResult> StateEditPopup(string id)
        {
            var sp = await _stateProvinceService.GetStateProvinceById(id);
            if (sp == null)
                //No state found with the specified id
                return RedirectToAction("List");

            var model = sp.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sp.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> StateEditPopup(StateProvinceModel model)
        {
            var sp = await _stateProvinceService.GetStateProvinceById(model.Id);
            if (sp == null)
                //No state found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                await _countryViewModelService.UpdateStateProvinceModel(sp, model);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> StateDelete(string id)
        {
            var state = await _stateProvinceService.GetStateProvinceById(id);
            if (state == null)
                throw new ArgumentException("No state found with the specified id");

            if (await _addressService.GetAddressTotalByStateProvinceId(state.Id) > 0)
            {
                return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Configuration.Countries.States.CantDeleteWithAddresses") });
            }
            if (ModelState.IsValid)
            {
                await _stateProvinceService.DeleteStateProvince(state);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Export / import
        [PermissionAuthorizeAction(PermissionActionName.Export)]
        public async Task<IActionResult> ExportCsv()
        {
            string fileName = String.Format("states_{0}_{1}.txt", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));

            var states = await _stateProvinceService.GetStateProvinces(true);
            string result = await _exportManager.ExportStatesToTxt(states);

            return File(Encoding.UTF8.GetBytes(result), "text/csv", fileName);
        }
        [PermissionAuthorizeAction(PermissionActionName.Import)]
        [HttpPost]
        public async Task<IActionResult> ImportCsv(IFormFile importcsvfile)
        {
            try
            {
                if (importcsvfile != null && importcsvfile.Length > 0)
                {
                    int count = await _importManager.ImportStatesFromTxt(importcsvfile.OpenReadStream());
                    SuccessNotification(String.Format(_localizationService.GetResource("Admin.Configuration.Countries.ImportSuccess"), count));
                    return RedirectToAction("List");
                }
                ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        #endregion
    }
}
