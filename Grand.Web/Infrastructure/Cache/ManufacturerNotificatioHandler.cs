using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class ManufacturerNotificatioHandler :
        INotificationHandler<EntityInserted<Manufacturer>>,
        INotificationHandler<EntityUpdated<Manufacturer>>,
        INotificationHandler<EntityDeleted<Manufacturer>>
    {

        private readonly ICacheManager _cacheManager;

        public ManufacturerNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
    }
}
