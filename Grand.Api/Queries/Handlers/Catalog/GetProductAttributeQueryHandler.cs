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
    public class GetProductAttributeQueryHandler : IRequestHandler<GetQuery<ProductAttributeDto>, IMongoQueryable<ProductAttributeDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetProductAttributeQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }
        public Task<IMongoQueryable<ProductAttributeDto>> Handle(GetQuery<ProductAttributeDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult(
                    _mongoDBContext.Database()
                    .GetCollection<ProductAttributeDto>
                    (typeof(Domain.Catalog.ProductAttribute).Name)
                    .AsQueryable());
            else
                return Task.FromResult(_mongoDBContext.Database()
                    .GetCollection<ProductAttributeDto>(typeof(Domain.Catalog.ProductAttribute).Name)
                    .AsQueryable()
                    .Where(x => x.Id == request.Id));
        }
    }
}
