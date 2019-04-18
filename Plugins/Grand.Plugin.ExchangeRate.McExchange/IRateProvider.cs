using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Plugin.ExchangeRate.McExchange
{
    interface IRateProvider
    {
        Task<IList<Core.Domain.Directory.ExchangeRate>> GetCurrencyLiveRates();
    }
}
