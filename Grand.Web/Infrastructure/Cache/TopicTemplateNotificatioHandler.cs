using Grand.Core.Caching;
using Grand.Domain.Topics;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public partial class TopicTemplateNotificatioHandler :
        INotificationHandler<EntityInserted<TopicTemplate>>,
        INotificationHandler<EntityUpdated<TopicTemplate>>,
        INotificationHandler<EntityDeleted<TopicTemplate>>
    {
        private readonly ICacheManager _cacheManager;

        public TopicTemplateNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_TEMPLATE_PATTERN_KEY);
        }

    }
}
