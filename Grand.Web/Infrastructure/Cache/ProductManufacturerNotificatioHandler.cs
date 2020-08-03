using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class ProductManufacturerNotificatioHandler :
        INotificationHandler<EntityInserted<ProductManufacturer>>,
        INotificationHandler<EntityUpdated<ProductManufacturer>>,
        INotificationHandler<EntityDeleted<ProductManufacturer>>
    {

        private readonly ICacheManager _cacheManager;

        public ProductManufacturerNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.ManufacturerId));
        }
        public async Task Handle(EntityUpdated<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.ManufacturerId));
        }
        public async Task Handle(EntityDeleted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.ManufacturerId));
        }
    }
}