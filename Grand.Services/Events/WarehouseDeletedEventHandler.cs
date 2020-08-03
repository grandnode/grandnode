using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Shipping;
using Grand.Core.Events;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public class WarehouseDeletedEventHandler : INotificationHandler<EntityDeleted<Warehouse>>
    {
        private readonly IRepository<Product> _productRepository;

        public WarehouseDeletedEventHandler(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task Handle(EntityDeleted<Warehouse> notification, CancellationToken cancellationToken)
        {
            var builder = Builders<Product>.Update;
            var updatefilter = builder.PullFilter(x => x.ProductWarehouseInventory, y => y.WarehouseId == notification.Entity.Id);

            await _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            var builder2 = Builders<Product>.Filter;
            var filter2 = builder2.Eq(x => x.WarehouseId, notification.Entity.Id);
            var update2 = Builders<Product>.Update
                .Set(x => x.WarehouseId, "");

            await _productRepository.Collection.UpdateManyAsync(filter2, update2);

        }
    }
}
