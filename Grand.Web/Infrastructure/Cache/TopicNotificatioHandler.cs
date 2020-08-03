using Grand.Core.Caching;
using Grand.Domain.Topics;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class TopicNotificatioHandler :
        INotificationHandler<EntityInserted<Topic>>,
        INotificationHandler<EntityUpdated<Topic>>,
        INotificationHandler<EntityDeleted<Topic>>
    {

        private readonly ICacheManager _cacheManager;

        public TopicNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
    }
}