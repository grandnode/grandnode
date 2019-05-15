using Grand.Core.Caching;
using Grand.Core.Events;
using Grand.Plugin.Tax.CountryStateZip.Domain;
using Grand.Services.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.Tax.CountryStateZip.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
        //tax rates
        IConsumer<EntityInserted<TaxRate>>,
        IConsumer<EntityUpdated<TaxRate>>,
        IConsumer<EntityDeleted<TaxRate>>
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        public const string ALL_TAX_RATES_MODEL_KEY = "Grand.plugins.tax.countrystatezip.all";
        public const string ALL_TAX_RATES_PATTERN_KEY = "Grand.plugins.tax.countrystatezip";

        private readonly ICacheManager _cacheManager;

        public ModelCacheEventConsumer(IServiceProvider serviceProvider)
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
        }

        //tax rates
        public async Task HandleEvent(EntityInserted<TaxRate> eventMessage)
        {
            await _cacheManager.RemoveByPattern(ALL_TAX_RATES_PATTERN_KEY);
        }
        public async Task HandleEvent(EntityUpdated<TaxRate> eventMessage)
        {
            await _cacheManager.RemoveByPattern(ALL_TAX_RATES_PATTERN_KEY);
        }
        public async Task HandleEvent(EntityDeleted<TaxRate> eventMessage)
        {
            await _cacheManager.RemoveByPattern(ALL_TAX_RATES_PATTERN_KEY);
        }
    }
}