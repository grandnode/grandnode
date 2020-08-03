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

        private readonly ICacheManager _cacheManager;

        public BlogPostNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
    }
}