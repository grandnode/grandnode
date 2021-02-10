using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class ProductCategoryNotificatioHandler :
        INotificationHandler<EntityInserted<ProductCategory>>,
        INotificationHandler<EntityUpdated<ProductCategory>>,
        INotificationHandler<EntityDeleted<ProductCategory>>
    {

        private readonly ICacheBase _cacheBase;

        public ProductCategoryNotificatioHandler(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        public async Task Handle(EntityInserted<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.CATEGORY_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.CategoryId));
        }
        public async Task Handle(EntityUpdated<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.CATEGORY_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.CategoryId));
        }
        public async Task Handle(EntityDeleted<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.CATEGORY_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.CategoryId));
        }
    }
}