using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class SpecificationAttributeNotificatioHandler :
        INotificationHandler<EntityUpdated<SpecificationAttribute>>,
        INotificationHandler<EntityDeleted<SpecificationAttribute>>
    {

        private readonly ICacheManager _cacheManager;

        public SpecificationAttributeNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        public async Task Handle(EntityUpdated<SpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_SPECS_PATTERN);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<SpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_SPECS_PATTERN);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }
    }
}