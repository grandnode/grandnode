using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public partial class CategoryTemplateNotificatioHandler :
        INotificationHandler<EntityInserted<CategoryTemplate>>,
        INotificationHandler<EntityUpdated<CategoryTemplate>>,
        INotificationHandler<EntityDeleted<CategoryTemplate>>
        
    {
        private readonly ICacheManager _cacheManager;

        public CategoryTemplateNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        
    }
}
