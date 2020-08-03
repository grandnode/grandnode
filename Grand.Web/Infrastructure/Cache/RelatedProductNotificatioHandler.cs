using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class RelatedProductNotificatioHandler :
        INotificationHandler<EntityInserted<RelatedProduct>>,
        INotificationHandler<EntityUpdated<RelatedProduct>>,
        INotificationHandler<EntityDeleted<RelatedProduct>>
    {

        private readonly ICacheManager _cacheManager;

        public RelatedProductNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.ProductId1));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.ProductId2));

        }
        public async Task Handle(EntityUpdated<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.ProductId1));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.ProductId2));
        }
        public async Task Handle(EntityDeleted<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.ProductId1));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.ProductId2));
        }
    }
}