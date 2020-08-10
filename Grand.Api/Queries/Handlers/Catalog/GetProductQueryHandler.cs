using Grand.Api.DTOs.Catalog;
using Grand.Api.Queries.Models.Common;
using Grand.Domain.Data;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Queries.Handlers.Common
{
    public class GetProductQueryHandler : IRequestHandler<GetQuery<ProductDto>, IMongoQueryable<ProductDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetProductQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }

        public Task<IMongoQueryable<ProductDto>> Handle(GetQuery<ProductDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult(
                    _mongoDBContext.Database()
                    .GetCollection<ProductDto>
                    (typeof(Domain.Catalog.Product).Name)
                    .AsQueryable());
            else
                return Task.FromResult(_mongoDBContext.Database()
                    .GetCollection<ProductDto>(typeof(Domain.Catalog.Product).Name)
                    .AsQueryable()
                    .Where(x => x.Id == request.Id));
        }
    }
}
