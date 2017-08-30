using Grand.Core.Domain.Directory;
using Grand.Core.Plugins;
using Grand.Services.Directory;
using System.Collections.Generic;

namespace Grand.Services.Tests.Directory
{
    public class TestExchangeRateProvider : BasePlugin, IExchangeRateProvider {
        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public IList<ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode) {
            return new List<ExchangeRate>();
        }
    }
}
