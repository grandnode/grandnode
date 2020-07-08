using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Commands.Models.Common;
using MediatR;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Common
{
    public class ClearMostViewedCommandHandler : IRequestHandler<ClearMostViewedCommand, bool>
    {
        private readonly IRepository<Product> _repositoryProduct;

        public ClearMostViewedCommandHandler(IRepository<Product> repositoryProduct)
        {
            _repositoryProduct = repositoryProduct;
        }

        public async Task<bool> Handle(ClearMostViewedCommand request, CancellationToken cancellationToken)
        {
            var update = new UpdateDefinitionBuilder<Product>().Set(x => x.Viewed, 0);
            await _repositoryProduct.Collection.UpdateManyAsync(x => x.Viewed != 0, update);
            return true;
        }
    }
}
