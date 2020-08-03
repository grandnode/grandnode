using Grand.Core.Caching;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.News;
using Grand.Domain.Orders;
using Grand.Domain.Polls;
using Grand.Domain.Topics;
using Grand.Domain.Vendors;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public partial class ManufacturerTemplateNotificatioHandler :
        INotificationHandler<EntityInserted<ManufacturerTemplate>>,
        INotificationHandler<EntityUpdated<ManufacturerTemplate>>,
        INotificationHandler<EntityDeleted<ManufacturerTemplate>>
    {
        private readonly ICacheManager _cacheManager;

        public ManufacturerTemplateNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }

    }
}
