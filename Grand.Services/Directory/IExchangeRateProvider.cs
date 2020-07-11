using Grand.Domain.Directory;
using Grand.Core.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Directory
{
    /// <summary>
    /// Exchange rate provider interface
    /// </summary>
    public partial interface IExchangeRateProvider : IPlugin
    {
        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        Task<IList<ExchangeRate>> GetCurrencyLiveRates(string exchangeRateCurrencyCode);
    }
}