using Grand.Core.Caching;
using Grand.Domain.Blogs;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class BlogPostNotificatioHandler :
        INotificationHandler<EntityInserted<BlogPost>>,
        INotificationHandler<EntityUpdated<BlogPost>>,
        INotificationHandler<EntityDeleted<BlogPost>>
    {

        private readonly ICacheBase _cacheBase;

        public BlogPostNotificatioHandler(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        public async Task Handle(EntityInserted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
    }
}