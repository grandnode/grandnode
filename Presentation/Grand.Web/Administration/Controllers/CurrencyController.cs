using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Directory;
using Grand.Core;
using Grand.Core.Domain.Directory;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Core.Domain.Localization;

namespace Grand.Admin.Controllers
{
    public partial class CurrencyController :  BaseAdminController
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ISettingService _settingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Constructors

        public CurrencyController(ICurrencyService currencyService, 
            CurrencySettings currencySettings, ISettingService settingService,
            IDateTimeHelper dateTimeHelper, ILocalizationService localizationService,
            IPermissionService permissionService,
            ILanguageService languageService,
            IStoreService storeService, 
            IStoreMappingService storeMappingService)
        {
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._settingService = settingService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._languageService = languageService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
        }
        
        #endregion

        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(Currency currency, CurrencyModel model)
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

        [NonAction]
        protected virtual void PrepareStoresMappingModel(CurrencyModel model, Currency currency, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (currency != null)
                {
                    model.SelectedStoreIds = currency.Stores.ToArray();
                }
            }
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List(bool liveRates = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            if (liveRates)
            {
                try
                {
                    var primaryExchangeCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);
                    if (primaryExchangeCurrency == null)
                        throw new GrandException("Primary exchange rate currency is not set");

                    ViewBag.Rates = _currencyService.GetCurrencyLiveRates(primaryExchangeCurrency.CurrencyCode);
                }
                catch (Exception exc)
                {
                    ErrorNotification(exc, false);
                }
            }
            ViewBag.ExchangeRateProviders = new List<SelectListItem>();
            foreach (var erp in _currencyService.LoadAllExchangeRateProviders())
            {
                ViewBag.ExchangeRateProviders.Add(new SelectListItem
                {
                    Text = erp.PluginDescriptor.FriendlyName,
                    Value = erp.PluginDescriptor.SystemName,
                    Selected = erp.PluginDescriptor.SystemName.Equals(_currencySettings.ActiveExchangeRateProviderSystemName, StringComparison.InvariantCultureIgnoreCase)
                });
            }
            ViewBag.AutoUpdateEnabled = _currencySettings.AutoUpdateEnabled;
           
            return View();
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult List(FormCollection formValues)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            _currencySettings.ActiveExchangeRateProviderSystemName = formValues["exchangeRateProvider"];
            _currencySettings.AutoUpdateEnabled = !formValues["autoUpdateEnabled"].Equals("false");
            _settingService.SaveSetting(_currencySettings);
            return RedirectToAction("List", "Currency");
        }

        [HttpPost]
        public ActionResult ListGrid(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currenciesModel = _currencyService.GetAllCurrencies(true).Select(x => x.ToModel()).ToList();
            foreach (var currency in currenciesModel)
                currency.IsPrimaryExchangeRateCurrency = currency.Id == _currencySettings.PrimaryExchangeRateCurrencyId;
            foreach (var currency in currenciesModel)
                currency.IsPrimaryStoreCurrency = currency.Id == _currencySettings.PrimaryStoreCurrencyId;

            var gridModel = new DataSourceResult
            {
                Data = currenciesModel,
                Total = currenciesModel.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ApplyRate(string currencyCode, decimal rate)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyByCode(currencyCode);
            if (currency != null)
            {
                currency.Rate = rate;
                currency.UpdatedOnUtc = DateTime.UtcNow;
                _currencyService.UpdateCurrency(currency);
            }
            return Json(new { result = true });
            //return RedirectToAction("List","Currency", new { liveRates = true });
        }

        [HttpPost]
        public ActionResult MarkAsPrimaryExchangeRateCurrency(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            _currencySettings.PrimaryExchangeRateCurrencyId = id;
            _settingService.SaveSetting(_currencySettings);

            return Json(new { result = true });
        }

        [HttpPost]
        public ActionResult MarkAsPrimaryStoreCurrency(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            _currencySettings.PrimaryStoreCurrencyId = id;
            _settingService.SaveSetting(_currencySettings);
            return Json(new { result = true });
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var model = new CurrencyModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //default values
            model.Published = true;
            model.Rate = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CurrencyModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var currency = model.ToEntity();
                currency.CreatedOnUtc = DateTime.UtcNow;
                currency.UpdatedOnUtc = DateTime.UtcNow;
                currency.Locales = UpdateLocales(currency, model);
                currency.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                _currencyService.InsertCurrency(currency);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Currencies.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = currency.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //Stores
            PrepareStoresMappingModel(model, null, true);

            return View(model);
        }
        
        public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyById(id);
            if (currency == null)
                //No currency found with the specified id
                return RedirectToAction("List");

            var model = currency.ToModel();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(currency.CreatedOnUtc, DateTimeKind.Utc);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = currency.GetLocalized(x => x.Name, languageId, false, false);
            });
            //Stores
            PrepareStoresMappingModel(model, currency, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CurrencyModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyById(model.Id);
            if (currency == null)
                //No currency found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //ensure we have at least one published language
                var allCurrencies = _currencyService.GetAllCurrencies();
                if (allCurrencies.Count == 1 && allCurrencies[0].Id == currency.Id &&
                    !model.Published)
                {
                    ErrorNotification("At least one published currency is required.");
                    return RedirectToAction("Edit", new { id = currency.Id });
                }

                currency = model.ToEntity(currency);
                currency.UpdatedOnUtc = DateTime.UtcNow;
                currency.Locales = UpdateLocales(currency, model);
                currency.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _currencyService.UpdateCurrency(currency);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Currencies.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = currency.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(currency.CreatedOnUtc, DateTimeKind.Utc);

            //Stores
            PrepareStoresMappingModel(model, currency, true);

            return View(model);
        }
        
        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyById(id);
            if (currency == null)
                //No currency found with the specified id
                return RedirectToAction("List");
            
            try
            {
                if (currency.Id == _currencySettings.PrimaryStoreCurrencyId)
                    throw new GrandException(_localizationService.GetResource("Admin.Configuration.Currencies.CantDeletePrimary"));

                if (currency.Id == _currencySettings.PrimaryExchangeRateCurrencyId)
                    throw new GrandException(_localizationService.GetResource("Admin.Configuration.Currencies.CantDeleteExchange"));

                //ensure we have at least one published currency
                var allCurrencies = _currencyService.GetAllCurrencies();
                if (allCurrencies.Count == 1 && allCurrencies[0].Id == currency.Id)
                {
                    ErrorNotification("At least one published currency is required.");
                    return RedirectToAction("Edit", new { id = currency.Id });
                }

                _currencyService.DeleteCurrency(currency);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Currencies.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = currency.Id });
            }
        }

        #endregion
    }
}
