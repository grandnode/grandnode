using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class SimilarProductNotificatioHandler :
        INotificationHandler<EntityInserted<SimilarProduct>>,
        INotificationHandler<EntityUpdated<SimilarProduct>>,
        INotificationHandler<EntityDeleted<SimilarProduct>>
    {

        private readonly ICacheManager _cacheManager;

        public SimilarProductNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<SimilarProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId1));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId2));
        }
        public async Task Handle(EntityUpdated<SimilarProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId1));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId2));
        }
        public async Task Handle(EntityDeleted<SimilarProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId1));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId2));
        }
    }
}