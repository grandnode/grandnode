using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Tasks;
using Grand.Services.Directory;
using System;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Represents a task for updating exchange rates
    /// </summary>
    public partial class UpdateExchangeRateScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly object _lock = new object();
        public UpdateExchangeRateScheduleTask(ICurrencyService currencyService, CurrencySettings currencySettings)
        {
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            lock (_lock)
            {
                if (!_currencySettings.AutoUpdateEnabled)
                    return;

                var primaryCurrencyCode = _currencyService.GetPrimaryExchangeRateCurrency().GetAwaiter().GetResult().CurrencyCode;
                var exchangeRates = _currencyService.GetCurrencyLiveRates(primaryCurrencyCode).GetAwaiter().GetResult();

                foreach (var exchageRate in exchangeRates)
                {
                    var currency = _currencyService.GetCurrencyByCode(exchageRate.CurrencyCode).GetAwaiter().GetResult();
                    if (currency != null)
                    {
                        currency.Rate = exchageRate.Rate;
                        currency.UpdatedOnUtc = DateTime.UtcNow;
                        _currencyService.UpdateCurrency(currency).GetAwaiter().GetResult();
                    }
                }
            }
        }
    }
}
