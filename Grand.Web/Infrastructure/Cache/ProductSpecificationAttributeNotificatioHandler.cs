using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class ProductSpecificationAttributeNotificatioHandler :
        INotificationHandler<EntityInserted<ProductSpecificationAttribute>>,
        INotificationHandler<EntityUpdated<ProductSpecificationAttribute>>,
        INotificationHandler<EntityDeleted<ProductSpecificationAttribute>>
    {

        private readonly ICacheManager _cacheManager;

        public ProductSpecificationAttributeNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<ProductSpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_SPECS_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductSpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_SPECS_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductSpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_SPECS_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }
    }
}