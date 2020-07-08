using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Tax;
using Grand.Core.Events;
using MediatR;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public class TaxCategoryDeletedEventHandler : INotificationHandler<EntityDeleted<TaxCategory>>
    {
        private readonly IRepository<Product> _productRepository;

        public TaxCategoryDeletedEventHandler(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task Handle(EntityDeleted<TaxCategory> notification, CancellationToken cancellationToken)
        {
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.TaxCategoryId, notification.Entity.Id);
            var update = Builders<Product>.Update
                .Set(x => x.TaxCategoryId, "");

            await _productRepository.Collection.UpdateManyAsync(filter, update);
        }
    }
}
