using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Directory;
using Grand.Core.Plugins;
using Grand.Services.Events;
using Grand.Services.Stores;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Directory
{
    /// <summary>
    /// Currency service
    /// </summary>
    public partial class CurrencyService : ICurrencyService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : currency ID
        /// </remarks>
        private const string CURRENCIES_BY_ID_KEY = "Grand.currency.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : currency code
        /// </remarks>
        private const string CURRENCIES_BY_CODE = "Grand.currency.code-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        private const string CURRENCIES_ALL_KEY = "Grand.currency.all-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CURRENCIES_PATTERN_KEY = "Grand.currency.";

        #endregion

        #region Fields

        private readonly IRepository<Currency> _currencyRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICacheManager _cacheManager;
        private readonly IPluginFinder _pluginFinder;
        private readonly IMediator _mediator;
        private readonly CurrencySettings _currencySettings;
        private Currency _primaryCurrency;
        private Currency _primaryExchangeRateCurrency;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="currencyRepository">Currency repository</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="currencySettings">Currency settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="mediator">Mediator</param>
        public CurrencyService(ICacheManager cacheManager,
            IRepository<Currency> currencyRepository,
            IStoreMappingService storeMappingService,
            CurrencySettings currencySettings,
            IPluginFinder pluginFinder,
            IMediator mediator)
        {
            _cacheManager = cacheManager;
            _currencyRepository = currencyRepository;
            _storeMappingService = storeMappingService;
            _currencySettings = currencySettings;
            _pluginFinder = pluginFinder;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public virtual Task<IList<ExchangeRate>> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            var exchangeRateProvider = LoadActiveExchangeRateProvider();
            if (exchangeRateProvider == null)
                throw new Exception("Active exchange rate provider cannot be loaded");
            return exchangeRateProvider.GetCurrencyLiveRates(exchangeRateCurrencyCode);
        }

        /// <summary>
        /// Deletes currency
        /// </summary>
        /// <param name="currency">Currency</param>
        public virtual async Task DeleteCurrency(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException("currency");

            await _currencyRepository.DeleteAsync(currency);

            await _cacheManager.RemoveByPrefix(CURRENCIES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(currency);
        }

        /// <summary>
        /// Gets a currency
        /// </summary>
        /// <param name="currencyId">Currency identifier</param>
        /// <returns>Currency</returns>
        public virtual Task<Currency> GetCurrencyById(string currencyId)
        {
            string key = string.Format(CURRENCIES_BY_ID_KEY, currencyId);
            return _cacheManager.GetAsync(key, () => _currencyRepository.GetByIdAsync(currencyId));
        }

        /// <summary>
        /// Gets primary store currency
        /// </summary>
        /// <returns>Currency</returns>
        public async Task<Currency> GetPrimaryStoreCurrency()
        {
            if (_primaryCurrency == null)
                _primaryCurrency = await GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);

            return _primaryCurrency;
        }

        /// <summary>
        /// Gets primary exchange currency
        /// </summary>
        /// <returns>Currency</returns>
        public async Task<Currency> GetPrimaryExchangeRateCurrency()
        {
            if (_primaryExchangeRateCurrency == null)
                _primaryExchangeRateCurrency = await GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);

            return _primaryExchangeRateCurrency;
        }

        /// <summary>
        /// Gets a currency by code
        /// </summary>
        /// <param name="currencyCode">Currency code</param>
        /// <returns>Currency</returns>
        public virtual async Task<Currency> GetCurrencyByCode(string currencyCode)
        {
            if (string.IsNullOrEmpty(currencyCode))
                return null;

            var key = string.Format(CURRENCIES_BY_CODE, currencyCode);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = from q in _currencyRepository.Table
                            where q.CurrencyCode.ToLowerInvariant() == currencyCode.ToLower()
                            select q;
                return query.FirstOrDefaultAsync();
            });
        }

        /// <summary>
        /// Gets all currencies
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Currencies</returns>
        public virtual async Task<IList<Currency>> GetAllCurrencies(bool showHidden = false, string storeId = "")
        {
            string key = string.Format(CURRENCIES_ALL_KEY, showHidden);
            var currencies = await _cacheManager.GetAsync(key, () =>
            {
                var query = _currencyRepository.Table;

                if (!showHidden)
                    query = query.Where(c => c.Published);
                query = query.OrderBy(c => c.DisplayOrder);
                return query.ToListAsync();
            });

            //store mapping
            if (!string.IsNullOrEmpty(storeId))
            {
                currencies = currencies.Where(c => _storeMappingService.Authorize(c, storeId)).ToList();
            }
            return currencies;
        }

        /// <summary>
        /// Inserts a currency
        /// </summary>
        /// <param name="currency">Currency</param>
        public virtual async Task InsertCurrency(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException("currency");

            await _currencyRepository.InsertAsync(currency);

            await _cacheManager.RemoveByPrefix(CURRENCIES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(currency);
        }

        /// <summary>
        /// Updates the currency
        /// </summary>
        /// <param name="currency">Currency</param>
        public virtual async Task UpdateCurrency(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException("currency");

            await _currencyRepository.UpdateAsync(currency);

            await _cacheManager.RemoveByPrefix(CURRENCIES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(currency);
        }



        /// <summary>
        /// Converts currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="exchangeRate">Currency exchange rate</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertCurrency(decimal amount, decimal exchangeRate)
        {
            if (amount != decimal.Zero && exchangeRate != decimal.Zero)
                return amount * exchangeRate;
            return decimal.Zero;
        }

        /// <summary>
        /// Converts currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrencyCode">Source currency code</param>
        /// <param name="targetCurrencyCode">Target currency code</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertCurrency(decimal amount, Currency sourceCurrencyCode, Currency targetCurrencyCode)
        {
            if (sourceCurrencyCode == null)
                throw new ArgumentNullException(nameof(sourceCurrencyCode));

            if (targetCurrencyCode == null)
                throw new ArgumentNullException(nameof(targetCurrencyCode));

            var result = amount;

            if (result == decimal.Zero || sourceCurrencyCode.Id == targetCurrencyCode.Id)
                return result;

            result = await ConvertToPrimaryExchangeRateCurrency(result, sourceCurrencyCode);
            result = await ConvertFromPrimaryExchangeRateCurrency(result, targetCurrencyCode);

            return result;

        }

        /// <summary>
        /// Converts to primary exchange rate currency 
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrencyCode">Source currency code</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertToPrimaryExchangeRateCurrency(decimal amount, Currency sourceCurrencyCode)
        {
            if (sourceCurrencyCode == null)
                throw new ArgumentNullException("sourceCurrencyCode");

            var primaryExchangeRateCurrency = await GetPrimaryExchangeRateCurrency();
            if (primaryExchangeRateCurrency == null)
                throw new Exception("Primary exchange rate currency cannot be loaded");

            decimal result = amount;
            decimal exchangeRate = sourceCurrencyCode.Rate;
            if (exchangeRate == decimal.Zero)
                throw new GrandException(string.Format("Exchange rate not found for currency [{0}]", sourceCurrencyCode.Name));
            result = result / exchangeRate;
            return result;
        }

        /// <summary>
        /// Converts from primary exchange rate currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="targetCurrencyCode">Target currency code</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertFromPrimaryExchangeRateCurrency(decimal amount, Currency targetCurrencyCode)
        {
            if (targetCurrencyCode == null)
                throw new ArgumentNullException("targetCurrencyCode");

            var primaryExchangeRateCurrency = await GetPrimaryExchangeRateCurrency();
            if (primaryExchangeRateCurrency == null)
                throw new Exception("Primary exchange rate currency cannot be loaded");

            decimal result = amount;

            decimal exchangeRate = targetCurrencyCode.Rate;
            if (exchangeRate == decimal.Zero)
                throw new GrandException(string.Format("Exchange rate not found for currency [{0}]", targetCurrencyCode.Name));

            result = result * exchangeRate;

            return result;
        }

        /// <summary>
        /// Converts to primary store currency 
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrencyCode">Source currency code</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertToPrimaryStoreCurrency(decimal amount, Currency sourceCurrencyCode)
        {
            if (sourceCurrencyCode == null)
                throw new ArgumentNullException("sourceCurrencyCode");

            var primaryStoreCurrency = await GetPrimaryStoreCurrency();
            var result = await ConvertCurrency(amount, sourceCurrencyCode, primaryStoreCurrency);

            return result;
        }

        /// <summary>
        /// Converts from primary store currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="targetCurrencyCode">Target currency code</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertFromPrimaryStoreCurrency(decimal amount, Currency targetCurrencyCode)
        {
            if (targetCurrencyCode == null)
                return amount;

            var result = await ConvertCurrency(amount, await GetPrimaryStoreCurrency(), targetCurrencyCode);
            return result;
        }


        /// <summary>
        /// Load active exchange rate provider
        /// </summary>
        /// <returns>Active exchange rate provider</returns>
        public virtual IExchangeRateProvider LoadActiveExchangeRateProvider()
        {
            var exchangeRateProvider = LoadExchangeRateProviderBySystemName(_currencySettings.ActiveExchangeRateProviderSystemName);
            if (exchangeRateProvider == null)
                exchangeRateProvider = LoadAllExchangeRateProviders().FirstOrDefault();
            return exchangeRateProvider;
        }

        /// <summary>
        /// Load exchange rate provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found exchange rate provider</returns>
        public virtual IExchangeRateProvider LoadExchangeRateProviderBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IExchangeRateProvider>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IExchangeRateProvider>(_pluginFinder.ServiceProvider);

            return null;
        }

        /// <summary>
        /// Load all exchange rate providers
        /// </summary>
        /// <returns>Exchange rate providers</returns>
        public virtual IList<IExchangeRateProvider> LoadAllExchangeRateProviders()
        {
            var exchangeRateProviders = _pluginFinder.GetPlugins<IExchangeRateProvider>();
            return exchangeRateProviders
                .OrderBy(tp => tp.PluginDescriptor)
                .ToList();
        }
        #endregion
    }
}