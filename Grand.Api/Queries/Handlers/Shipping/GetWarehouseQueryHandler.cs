using Grand.Api.DTOs.Shipping;
using Grand.Api.Queries.Models.Common;
using Grand.Domain.Data;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Queries.Handlers.Shipping
{
    public class GetWarehouseQueryHandler : IRequestHandler<GetQuery<WarehouseDto>, IMongoQueryable<WarehouseDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetWarehouseQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }
        public Task<IMongoQueryable<WarehouseDto>> Handle(GetQuery<WarehouseDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult
                    (_mongoDBContext.Database()
                    .GetCollection<WarehouseDto>
                    (typeof(Domain.Shipping.Warehouse).Name)
                    .AsQueryable());
            else
                return Task.FromResult
                    (_mongoDBContext.Database()
                    .GetCollection<WarehouseDto>
                    (typeof(Domain.Shipping.Warehouse).Name)
                    .AsQueryable()
                    .Where(x => x.Id == request.Id));
        }
    }
}