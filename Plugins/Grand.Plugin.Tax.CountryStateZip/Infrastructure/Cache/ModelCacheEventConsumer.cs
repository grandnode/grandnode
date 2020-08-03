using Grand.Core.Caching;
using Grand.Core.Events;
using Grand.Plugin.Tax.CountryStateZip.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Plugin.Tax.CountryStateZip.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
        //tax rates
        INotificationHandler<EntityInserted<TaxRate>>,
        INotificationHandler<EntityUpdated<TaxRate>>,
        INotificationHandler<EntityDeleted<TaxRate>>
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
        public async Task Handle(EntityInserted<TaxRate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ALL_TAX_RATES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<TaxRate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ALL_TAX_RATES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<TaxRate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ALL_TAX_RATES_PATTERN_KEY);
        }
    }
}