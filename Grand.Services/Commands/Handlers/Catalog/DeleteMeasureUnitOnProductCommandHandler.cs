using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Commands.Models.Catalog;
using MediatR;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Catalog
{
    public class DeleteMeasureUnitOnProductCommandHandler : IRequestHandler<DeleteMeasureUnitOnProductCommand, bool>
    {
        private readonly IRepository<Product> _repositoryProduct;

        public DeleteMeasureUnitOnProductCommandHandler(IRepository<Product> repositoryProduct)
        {
            _repositoryProduct = repositoryProduct;
        }

        public async Task<bool> Handle(DeleteMeasureUnitOnProductCommand request, CancellationToken cancellationToken)
        {
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.UnitId, request.MeasureUnitId);
            var update = Builders<Product>.Update
                .Set(x => x.UnitId, "");

            await _repositoryProduct.Collection.UpdateManyAsync(filter, update);
            return true;
        }
    }
}
