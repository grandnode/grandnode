using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class CategoryNotificatioHandler :
        INotificationHandler<EntityInserted<Category>>,
        INotificationHandler<EntityUpdated<Category>>,
        INotificationHandler<EntityDeleted<Category>>
    {

        private readonly ICacheManager _cacheManager;

        public CategoryNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }

        public async Task Handle(EntityDeleted<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
    }
}