using Grand.Core.Caching;
using Grand.Domain.Polls;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class PollNotificatioHandler :
        INotificationHandler<EntityInserted<Poll>>,
        INotificationHandler<EntityUpdated<Poll>>,
        INotificationHandler<EntityDeleted<Poll>>
    {

        private readonly ICacheBase _cacheBase;

        public PollNotificatioHandler(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        public async Task Handle(EntityInserted<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.POLLS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.POLLS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.POLLS_PATTERN_KEY);
        }
    }
}