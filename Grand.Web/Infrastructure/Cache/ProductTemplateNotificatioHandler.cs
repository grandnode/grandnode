using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public partial class ProductTemplateNotificatioHandler :
        INotificationHandler<EntityInserted<ProductTemplate>>,
        INotificationHandler<EntityUpdated<ProductTemplate>>,
        INotificationHandler<EntityDeleted<ProductTemplate>>
    {
        private readonly ICacheManager _cacheManager;

        public ProductTemplateNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }


        public async Task Handle(EntityInserted<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        
    }
}
