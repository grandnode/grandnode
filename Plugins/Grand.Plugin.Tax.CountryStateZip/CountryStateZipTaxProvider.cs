using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Plugins;
using Grand.Plugin.Tax.CountryStateZip.Infrastructure.Cache;
using Grand.Plugin.Tax.CountryStateZip.Services;
using Grand.Services.Localization;
using Grand.Services.Tax;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Tax.CountryStateZip
{
    /// <summary>
    /// Fixed rate tax provider
    /// </summary>
    public class CountryStateZipTaxProvider : BasePlugin, ITaxProvider
    {
        private readonly ITaxRateService _taxRateService;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public CountryStateZipTaxProvider(ITaxRateService taxRateService,
            IStoreContext storeContext,
            ICacheManager cacheManager,
            IWebHelper webHelper,
            ILocalizationService localizationService, 
            ILanguageService languageService)
        {
            _taxRateService = taxRateService;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/TaxCountryStateZip/Configure";
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        public async Task<CalculateTaxResult> GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult();

            if (calculateTaxRequest.Address == null)
            {
                result.Errors.Add("Address is not set");
                return result;
            }

            const string cacheKey = ModelCacheEventConsumer.ALL_TAX_RATES_MODEL_KEY;
            var allTaxRates = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    var taxes = await _taxRateService.GetAllTaxRates();
                    return taxes.Select(x => new TaxRateForCaching
                    {
                        Id = x.Id,
                        StoreId = x.StoreId,
                        TaxCategoryId = x.TaxCategoryId,
                        CountryId = x.CountryId,
                        StateProvinceId = x.StateProvinceId,
                        Zip = x.Zip,
                        Percentage = x.Percentage
                    });
                });

            var storeId = _storeContext.CurrentStore.Id;
            var taxCategoryId = calculateTaxRequest.TaxCategoryId;
            var countryId = calculateTaxRequest.Address.CountryId;
            var stateProvinceId = calculateTaxRequest.Address.StateProvinceId;
            var zip = calculateTaxRequest.Address.ZipPostalCode;

            if (zip == null)
                zip = string.Empty;
            zip = zip.Trim();

            var existingRates = allTaxRates.Where(taxRate => taxRate.CountryId == countryId && taxRate.TaxCategoryId == taxCategoryId).ToList();

            //filter by store
            var matchedByStore = existingRates.Where(taxRate => storeId == taxRate.StoreId).ToList();

            //not found? use the default ones (ID == 0)
            if (!matchedByStore.Any())
                matchedByStore.AddRange(existingRates.Where(taxRate => string.IsNullOrEmpty(taxRate.StoreId)));

            //filter by state/province
            var matchedByStateProvince = matchedByStore.Where(taxRate => stateProvinceId == taxRate.StateProvinceId).ToList();

            //not found? use the default ones (ID == 0)
            if (!matchedByStateProvince.Any())
                matchedByStateProvince.AddRange(matchedByStore.Where(taxRate => string.IsNullOrEmpty(taxRate.StateProvinceId)));

            //filter by zip
            var matchedByZip = matchedByStateProvince.Where(taxRate => (string.IsNullOrEmpty(zip) && string.IsNullOrEmpty(taxRate.Zip)) || zip.Equals(taxRate.Zip, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!matchedByZip.Any())
                matchedByZip.AddRange(matchedByStateProvince.Where(taxRate => string.IsNullOrWhiteSpace(taxRate.Zip)));

            if (matchedByZip.Any())
                result.TaxRate = matchedByZip[0].Percentage;

            return result;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task Install()
        {
            //locales
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Store", "Store");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Store.Hint", "If an asterisk is selected, then this shipping rate will apply to all stores.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Country", "Country");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Country.Hint", "The country.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.StateProvince", "State / province");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.StateProvince.Hint", "If an asterisk is selected, then this tax rate will apply to all customers from the given country, regardless of the state.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Zip", "Zip");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Zip.Hint", "Zip / postal code. If zip is empty, then this tax rate will apply to all customers from the given country or state, regardless of the zip code.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.TaxCategory", "Tax category");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.TaxCategory.Hint", "The tax category.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Percentage", "Percentage");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Percentage.Hint", "The tax rate.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.AddRecord", "Add tax rate");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.AddRecord.Hint", "Adding a new tax rate");

            await base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task Uninstall()
        {
            //locales
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Store");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Store.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Country");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Country.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.StateProvince");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.StateProvince.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Zip");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Zip.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.TaxCategory");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.TaxCategory.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Percentage");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.Fields.Percentage.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.AddRecord");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.CountryStateZip.AddRecord.Hint");

            await base.Uninstall();
        }
    }
}