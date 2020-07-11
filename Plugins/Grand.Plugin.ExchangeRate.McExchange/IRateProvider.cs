using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Plugin.ExchangeRate.McExchange
{
    internal interface IRateProvider
    {
        Task<IList<Domain.Directory.ExchangeRate>> GetCurrencyLiveRates();
    }
}
