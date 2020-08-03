using Grand.Core.Caching;
using Grand.Domain.Orders;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class CheckoutAttributeNotificatioHandler :
        INotificationHandler<EntityInserted<CheckoutAttribute>>,
        INotificationHandler<EntityUpdated<CheckoutAttribute>>,
        INotificationHandler<EntityDeleted<CheckoutAttribute>>
    {

        private readonly ICacheManager _cacheManager;

        public CheckoutAttributeNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CHECKOUTATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CHECKOUTATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CHECKOUTATTRIBUTES_PATTERN_KEY);
        }
    }
}