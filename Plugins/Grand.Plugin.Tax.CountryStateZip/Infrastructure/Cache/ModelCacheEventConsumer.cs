using Grand.Core.Caching;
using Grand.Core.Events;
using Grand.Core.Infrastructure;
using Grand.Plugin.Tax.CountryStateZip.Domain;
using Grand.Services.Events;

namespace Grand.Plugin.Tax.CountryStateZip.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer: 
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
        
        public ModelCacheEventConsumer()
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.Resolve<ICacheManager>();
        }

        //tax rates
        public void HandleEvent(EntityInserted<TaxRate> eventMessage)
        {
            _cacheManager.RemoveByPattern(ALL_TAX_RATES_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<TaxRate> eventMessage)
        {
            _cacheManager.RemoveByPattern(ALL_TAX_RATES_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<TaxRate> eventMessage)
        {
            _cacheManager.RemoveByPattern(ALL_TAX_RATES_PATTERN_KEY);
        }
    }
}