using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Tax;
using Grand.Core.Plugins;
using Grand.Core;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.Tax.FixedRate
{
    /// <summary>
    /// Fixed rate tax provider
    /// </summary>
    public class FixedRateTaxProvider : BasePlugin, ITaxProvider
    {
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public FixedRateTaxProvider(ISettingService settingService, IWebHelper webHelper, ILocalizationService localizationService, ILanguageService languageService)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _languageService = languageService;
        }
        
        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        public Task<CalculateTaxResult> GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult
            {
                TaxRate = GetTaxRate(calculateTaxRequest.TaxCategoryId)
            };
            return Task.FromResult(result);
        }

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="taxCategoryId">The tax category identifier</param>
        /// <returns>Tax rate</returns>
        protected decimal GetTaxRate(string taxCategoryId)
        {
            var rate = _settingService.GetSettingByKey<decimal>(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId));
            return rate;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/TaxFixedRate/Configure";
        }
        
        public override async Task Install()
        {
            //locales
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.FixedRate.Fields.TaxCategoryName", "Tax category");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.FixedRate.Fields.Rate", "Rate");
            
            await base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task Uninstall()
        {
            //locales
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.FixedRate.Fields.TaxCategoryName");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Tax.FixedRate.Fields.Rate");
            
            await base.Uninstall();
        }
    }
}
