using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Framework.Security.Authorization;
using Grand.Plugin.Tax.CountryStateZip.Domain;
using Grand.Plugin.Tax.CountryStateZip.Models;
using Grand.Plugin.Tax.CountryStateZip.Services;
using Grand.Services.Directory;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Tax.CountryStateZip.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    [PermissionAuthorize(PermissionSystemName.TaxSettings)]
    public class TaxCountryStateZipController : BasePluginController
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ITaxRateService _taxRateService;
        private readonly IStoreService _storeService;

        public TaxCountryStateZipController(ITaxCategoryService taxCategoryService,
            ICountryService countryService, 
            IStateProvinceService stateProvinceService,
            ITaxRateService taxRateService,
            IStoreService storeService)
        {
            _taxCategoryService = taxCategoryService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _taxRateService = taxRateService;
            _storeService = storeService;
        }
        
        public async Task<IActionResult> Configure()
        {
            var taxCategories = await _taxCategoryService.GetAllTaxCategories();
            if (taxCategories.Count == 0)
                return Content("No tax categories can be loaded");
            
            var model = new TaxRateListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "" });
            var stores = await _storeService.GetAllStores();
            foreach (var s in stores)
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });
            //tax categories
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString() });
            //countries
            var countries = await _countryService.GetAllCountries(showHidden:true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "" });
            var defaultCountry = countries.FirstOrDefault();
            if (defaultCountry != null)
            {
                var states = await _stateProvinceService.GetStateProvincesByCountryId(defaultCountry.Id);
                foreach (var s in states)
                    model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            return View("~/Plugins/Tax.CountryStateZip/Views/TaxCountryStateZip/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RatesList(DataSourceRequest command)
        {
            var records = await _taxRateService.GetAllTaxRates(command.Page - 1, command.PageSize);
            var taxRatesModel = new List<TaxRateModel>();
            foreach (var x in records)
            {
                var m = new TaxRateModel
                {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    TaxCategoryId = x.TaxCategoryId,
                    CountryId = x.CountryId,
                    StateProvinceId = x.StateProvinceId,
                    Zip = x.Zip,
                    Percentage = x.Percentage,
                };
                //store
                var store = await _storeService.GetStoreById(x.StoreId);
                m.StoreName = (store != null) ? store.Shortcut : "*";
                //tax category
                var tc = await _taxCategoryService.GetTaxCategoryById(x.TaxCategoryId);
                m.TaxCategoryName = (tc != null) ? tc.Name : "";
                //country
                var c = await _countryService.GetCountryById(x.CountryId);
                m.CountryName = (c != null) ? c.Name : "Unavailable";
                //state
                var s = await _stateProvinceService.GetStateProvinceById(x.StateProvinceId);
                m.StateProvinceName = (s != null) ? s.Name : "*";
                //zip
                m.Zip = (!String.IsNullOrEmpty(x.Zip)) ? x.Zip : "*";
                taxRatesModel.Add(m);
            }

            var gridModel = new DataSourceResult
            {
                Data = taxRatesModel,
                Total = records.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RateUpdate(TaxRateModel model)
        {
            var taxRate = await _taxRateService.GetTaxRateById(model.Id);
            taxRate.Zip = model.Zip == "*" ? null : model.Zip;
            taxRate.Percentage = model.Percentage;
            await _taxRateService.UpdateTaxRate(taxRate);

            return new NullJsonResult();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RateDelete(string id)
        {
            var taxRate = await _taxRateService.GetTaxRateById(id);
            if (taxRate != null)
                await _taxRateService.DeleteTaxRate(taxRate);

            return new NullJsonResult();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddTaxRate(TaxRateListModel model)
        {
            var taxRate = new TaxRate
            {
                StoreId = model.AddStoreId,
                TaxCategoryId = model.AddTaxCategoryId,
                CountryId = model.AddCountryId,
                StateProvinceId = model.AddStateProvinceId,
                Zip = model.AddZip,
                Percentage = model.AddPercentage
            };
            await _taxRateService.InsertTaxRate(taxRate);

            return Json(new { Result = true });
        }
    }
}