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
    public class ManufacturerDeletedEventHandler : INotificationHandler<EntityDeleted<Manufacturer>>
    {
        private readonly IRepository<UrlRecord> _urlRecordRepository;

        public ManufacturerDeletedEventHandler(
            IRepository<UrlRecord> urlRecordRepository)
        {
            _urlRecordRepository = urlRecordRepository;
        }

        public async Task Handle(EntityDeleted<Manufacturer> notification, CancellationToken cancellationToken)
        {
            //delete url
            var filters = Builders<UrlRecord>.Filter;
            var filter = filters.Eq(x => x.EntityId, notification.Entity.Id);
            filter = filter & filters.Eq(x => x.EntityName, "Manufacturer");
            await _urlRecordRepository.Collection.DeleteManyAsync(filter);
        }
    }
}