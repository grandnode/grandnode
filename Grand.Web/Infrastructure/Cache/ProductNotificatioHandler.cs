using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class ProductNotificatioHandler :
        INotificationHandler<EntityInserted<Product>>,
        INotificationHandler<EntityUpdated<Product>>,
        INotificationHandler<EntityDeleted<Product>>
    {

        private readonly ICacheManager _cacheManager;

        public ProductNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);

            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.Id));
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }
        public async Task Handle(EntityDeleted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);

            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.Id));
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }
    }
}