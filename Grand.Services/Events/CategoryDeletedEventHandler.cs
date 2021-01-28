using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Seo;
using Grand.Core.Events;
using MediatR;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;


namespace Grand.Services.Events
{
    public class CategoryDeletedEventHandler : INotificationHandler<EntityDeleted<Category>>
    {
        private readonly IRepository<UrlRecord> _urlRecordRepository;

        public CategoryDeletedEventHandler(
            IRepository<UrlRecord> urlRecordRepository)
        {
            _urlRecordRepository = urlRecordRepository;
        }

        public async Task Handle(EntityDeleted<Category> notification, CancellationToken cancellationToken)
        {
            //delete url
            var filters = Builders<UrlRecord>.Filter;
            var filter = filters.Eq(x => x.EntityId, notification.Entity.Id);
            filter = filter & filters.Eq(x => x.EntityName, "Category");
            await _urlRecordRepository.Collection.DeleteManyAsync(filter);
        }
    }
}