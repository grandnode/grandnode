using Grand.Core.Caching;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Polls;
using Grand.Core.Domain.Topics;
using Grand.Core.Domain.Vendors;
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
