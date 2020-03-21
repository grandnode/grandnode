using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Web.Areas.Admin.Commands.Model.Common;
using MediatR;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Commands.Handler.Common
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
