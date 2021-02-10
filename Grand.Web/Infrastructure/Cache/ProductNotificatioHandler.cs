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

        private readonly ICacheBase _cacheBase;

        public ProductNotificatioHandler(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        public async Task Handle(EntityInserted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);

            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.Id));
            await _cacheBase.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }
        public async Task Handle(EntityDeleted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);

            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.Id));
            await _cacheBase.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }
    }
}