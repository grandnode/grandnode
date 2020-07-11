using Grand.Core.Caching;
using Grand.Domain.Orders;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class OrderNotificatioHandler :
        INotificationHandler<EntityInserted<Order>>,
        INotificationHandler<EntityUpdated<Order>>,
        INotificationHandler<EntityDeleted<Order>>
    {

        private readonly ICacheManager _cacheManager;

        public OrderNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
    }
}