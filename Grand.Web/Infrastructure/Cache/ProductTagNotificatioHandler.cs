using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class ProductTagNotificatioHandler :
        INotificationHandler<EntityInserted<ProductTag>>,
        INotificationHandler<EntityUpdated<ProductTag>>,
        INotificationHandler<EntityDeleted<ProductTag>>
    {

        private readonly ICacheManager _cacheManager;

        public ProductTagNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        public async Task Handle(EntityInserted<ProductTag> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_POPULAR_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductTag> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_POPULAR_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductTag> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_POPULAR_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
    }
}