using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Grand.Core;
using Grand.Core.Plugins;
using Grand.Services.Directory;
using Grand.Services.Localization;

[assembly:InternalsVisibleTo("Grand.Plugin.Tests")]

namespace Grand.Plugin.ExchangeRate.McExchange
{
    public class McExchangeRateProvider : BasePlugin, IExchangeRateProvider
    {
        private readonly ILocalizationService _localizationService;
        private readonly IServiceProvider _serviceProvider;

        private readonly IDictionary<string, IRateProvider> _providers = new Dictionary<string, IRateProvider> {
            {"eur", new EcbExchange()},
            {"pln", new NbpExchange()},
        };

        public McExchangeRateProvider(ILocalizationService localizationService, IServiceProvider serviceProvider)
        {
            _localizationService = localizationService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public Task<IList<Core.Domain.Directory.ExchangeRate>> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            if(string.IsNullOrEmpty(exchangeRateCurrencyCode))
                throw new ArgumentNullException(nameof(exchangeRateCurrencyCode));

            exchangeRateCurrencyCode = exchangeRateCurrencyCode.ToLowerInvariant();

            if (_providers.TryGetValue(exchangeRateCurrencyCode, out var provider))
            {
                return provider.GetCurrencyLiveRates();
            }

            throw new GrandException(_localizationService.GetResource("Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO"));
        }

        public override async Task Install()
        {
            //locales
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO", "You can use ECB (European central bank) exchange rate provider only when the primary exchange rate currency is set to EURO");
            await base.Install();
        }

        public override async Task Uninstall()
        {
            //locales
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO");
            await base.Uninstall();
        }
    }
}
