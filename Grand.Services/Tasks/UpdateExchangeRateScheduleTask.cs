using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Tasks;
using Grand.Services.Directory;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Represents a task for updating exchange rates
    /// </summary>
    public partial class UpdateExchangeRateScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        public UpdateExchangeRateScheduleTask(ICurrencyService currencyService, CurrencySettings currencySettings)
        {
            _currencyService = currencyService;
            _currencySettings = currencySettings;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task Execute()
        {
            if (!_currencySettings.AutoUpdateEnabled)
                return;

            var primaryCurrency = (await _currencyService.GetPrimaryExchangeRateCurrency());

            if (primaryCurrency != null)
            {
                var exchangeRates = await _currencyService.GetCurrencyLiveRates(primaryCurrency.CurrencyCode);

                foreach (var exchageRate in exchangeRates)
                {
                    var currency = await _currencyService.GetCurrencyByCode(exchageRate.CurrencyCode);
                    if (currency != null && exchageRate.Rate > 0)
                    {
                        currency.Rate = primaryCurrency.Rate / exchageRate.Rate;
                        currency.UpdatedOnUtc = DateTime.UtcNow;
                        await _currencyService.UpdateCurrency(currency);
                    }
                }
            }
        }
    }
}
