using Grand.Core.Caching;
using Grand.Domain.Orders;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class ReturnRequestReasonNotificatioHandler :
        INotificationHandler<EntityInserted<ReturnRequestReason>>,
        INotificationHandler<EntityUpdated<ReturnRequestReason>>,
        INotificationHandler<EntityDeleted<ReturnRequestReason>>
    {

        private readonly ICacheBase _cacheBase;

        public ReturnRequestReasonNotificatioHandler(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        public async Task Handle(EntityInserted<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTREASONS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTREASONS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTREASONS_PATTERN_KEY);
        }
    }
}