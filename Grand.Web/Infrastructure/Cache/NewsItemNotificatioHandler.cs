using Grand.Core.Caching;
using Grand.Domain.News;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class NewsItemNotificatioHandler :
        INotificationHandler<EntityInserted<NewsItem>>,
        INotificationHandler<EntityUpdated<NewsItem>>,
        INotificationHandler<EntityDeleted<NewsItem>>
    {

        private readonly ICacheBase _cacheBase;

        public NewsItemNotificatioHandler(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        public async Task Handle(EntityInserted<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.NEWS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.NEWS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.NEWS_PATTERN_KEY);
        }
    }
}