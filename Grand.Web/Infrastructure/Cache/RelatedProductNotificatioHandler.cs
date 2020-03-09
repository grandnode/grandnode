﻿using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    public class RelatedProductNotificatioHandler :
        INotificationHandler<EntityInserted<RelatedProduct>>,
        INotificationHandler<EntityUpdated<RelatedProduct>>,
        INotificationHandler<EntityDeleted<RelatedProduct>>
    {

        private readonly ICacheManager _cacheManager;

        public RelatedProductNotificatioHandler(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Handle(EntityInserted<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));

        }
        public async Task Handle(EntityUpdated<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }
        public async Task Handle(EntityDeleted<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }
    }
}