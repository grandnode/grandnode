using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Plugins;
using Grand.Plugin.Tax.CountryStateZip.Infrastructure.Cache;
using Grand.Plugin.Tax.CountryStateZip.Services;
using Grand.Services.Localization;
using Grand.Services.Tax;
using System;
using System.Linq;

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

        public CountryStateZipTaxProvider(ITaxRateService taxRateService,
            IStoreContext storeContext,
            ICacheManager cacheManager,
            IWebHelper webHelper)
        {
            this._taxRateService = taxRateService;
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._webHelper = webHelper;
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
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult();

            if (calculateTaxRequest.Address == null)
            {
                result.Errors.Add("Address is not set");
                return result;
            }

            const string cacheKey = ModelCacheEventConsumer.ALL_TAX_RATES_MODEL_KEY;
            var allTaxRates = _cacheManager.Get(cacheKey, () =>
                _taxRateService
                    .GetAllTaxRates()
                    .Select(x => new TaxRateForCaching
                    {
                        Id = x.Id,
                        StoreId = x.StoreId,
                        TaxCategoryId = x.TaxCategoryId,
                        CountryId = x.CountryId,
                        StateProvinceId = x.StateProvinceId,
                        Zip = x.Zip,
                        Percentage = x.Percentage
                    }
                    )
                    .ToList()
                );

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
        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Store", "Store");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Store.Hint", "If an asterisk is selected, then this shipping rate will apply to all stores.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Country", "Country");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Country.Hint", "The country.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.StateProvince", "State / province");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.StateProvince.Hint", "If an asterisk is selected, then this tax rate will apply to all customers from the given country, regardless of the state.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Zip", "Zip");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Zip.Hint", "Zip / postal code. If zip is empty, then this tax rate will apply to all customers from the given country or state, regardless of the zip code.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.TaxCategory", "Tax category");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.TaxCategory.Hint", "The tax category.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Percentage", "Percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Percentage.Hint", "The tax rate.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.AddRecord", "Add tax rate");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.CountryStateZip.AddRecord.Hint", "Adding a new tax rate");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Store");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Store.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Country");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Country.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.StateProvince");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.StateProvince.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Zip");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Zip.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.TaxCategory");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.TaxCategory.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Percentage");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.Fields.Percentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.AddRecord");
            this.DeletePluginLocaleResource("Plugins.Tax.CountryStateZip.AddRecord.Hint");

            base.Uninstall();
        }
    }
}