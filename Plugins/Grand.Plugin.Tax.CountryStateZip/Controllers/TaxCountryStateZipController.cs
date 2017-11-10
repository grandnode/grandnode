using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
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
using System.Linq;

namespace Grand.Plugin.Tax.CountryStateZip.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public class TaxCountryStateZipController : BasePluginController
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ITaxRateService _taxRateService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;

        public TaxCountryStateZipController(ITaxCategoryService taxCategoryService,
            ICountryService countryService, 
            IStateProvinceService stateProvinceService,
            ITaxRateService taxRateService,
            IPermissionService permissionService,
            IStoreService storeService)
        {
            this._taxCategoryService = taxCategoryService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._taxRateService = taxRateService;
            this._permissionService = permissionService;
            this._storeService = storeService;
        }
        
        public IActionResult Configure()
        {
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            if (taxCategories.Count == 0)
                return Content("No tax categories can be loaded");
            
            var model = new TaxRateListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "" });
            var stores = _storeService.GetAllStores();
            foreach (var s in stores)
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            //tax categories
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString() });
            //countries
            var countries = _countryService.GetAllCountries(showHidden:true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "" });
            var defaultCountry = countries.FirstOrDefault();
            if (defaultCountry != null)
            {
                var states = _stateProvinceService.GetStateProvincesByCountryId(defaultCountry.Id);
                foreach (var s in states)
                    model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            return View("~/Plugins/Tax.CountryStateZip/Views/TaxCountryStateZip/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult RatesList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return Content("Access denied");

            var records = _taxRateService.GetAllTaxRates(command.Page - 1, command.PageSize);
            var taxRatesModel = records
                .Select(x =>
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
                    var store = _storeService.GetStoreById(x.StoreId);
                    m.StoreName = (store != null) ? store.Name : "*";
                    //tax category
                    var tc = _taxCategoryService.GetTaxCategoryById(x.TaxCategoryId);
                    m.TaxCategoryName = (tc != null) ? tc.Name : "";
                    //country
                    var c = _countryService.GetCountryById(x.CountryId);
                    m.CountryName = (c != null) ? c.Name : "Unavailable";
                    //state
                    var s = _stateProvinceService.GetStateProvinceById(x.StateProvinceId);
                    m.StateProvinceName = (s != null) ? s.Name : "*";
                    //zip
                    m.Zip = (!String.IsNullOrEmpty(x.Zip)) ? x.Zip : "*";
                    return m;
                })
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = taxRatesModel,
                Total = records.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult RateUpdate(TaxRateModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return Content("Access denied");

            var taxRate = _taxRateService.GetTaxRateById(model.Id);
            taxRate.Zip = model.Zip == "*" ? null : model.Zip;
            taxRate.Percentage = model.Percentage;
            _taxRateService.UpdateTaxRate(taxRate);

            return new NullJsonResult();
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult RateDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return Content("Access denied");

            var taxRate = _taxRateService.GetTaxRateById(id);
            if (taxRate != null)
                _taxRateService.DeleteTaxRate(taxRate);

            return new NullJsonResult();
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult AddTaxRate(TaxRateListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return Content("Access denied");

            var taxRate = new TaxRate
            {
                StoreId = model.AddStoreId,
                TaxCategoryId = model.AddTaxCategoryId,
                CountryId = model.AddCountryId,
                StateProvinceId = model.AddStateProvinceId,
                Zip = model.AddZip,
                Percentage = model.AddPercentage
            };
            _taxRateService.InsertTaxRate(taxRate);

            return Json(new { Result = true });
        }
    }
}