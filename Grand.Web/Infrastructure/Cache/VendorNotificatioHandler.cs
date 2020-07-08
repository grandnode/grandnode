using Grand.Core.Caching;
using Grand.Domain.Vendors;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class VendorNotificatioHandler :
        INotificationHandler<EntityInserted<Vendor>>,
        INotificationHandler<EntityUpdated<Vendor>>,
        INotificationHandler<EntityDeleted<Vendor>>
    {

        private readonly ICacheManager _cacheManager;

        public VendorNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.VENDOR_NAVIGATION_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.VENDOR_NAVIGATION_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.VENDOR_NAVIGATION_PATTERN_KEY);
        }
    }
}