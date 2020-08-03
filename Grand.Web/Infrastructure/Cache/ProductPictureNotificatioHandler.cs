using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class ProductPictureNotificatioHandler :
        INotificationHandler<EntityInserted<ProductPicture>>,
        INotificationHandler<EntityUpdated<ProductPicture>>,
        INotificationHandler<EntityDeleted<ProductPicture>>
    {

        private readonly ICacheManager _cacheManager;

        public ProductPictureNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }
        public async Task Handle(EntityUpdated<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }
        public async Task Handle(EntityDeleted<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }
    }
}