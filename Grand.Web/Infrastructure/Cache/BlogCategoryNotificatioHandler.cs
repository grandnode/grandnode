using Grand.Core.Caching;
using Grand.Domain.Blogs;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class BlogCategoryNotificatioHandler :
        INotificationHandler<EntityInserted<BlogCategory>>,
        INotificationHandler<EntityUpdated<BlogCategory>>,
        INotificationHandler<EntityDeleted<BlogCategory>>
    {

        private readonly ICacheBase _cacheBase;

        public BlogCategoryNotificatioHandler(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        public async Task Handle(EntityInserted<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
    }
}