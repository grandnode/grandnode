using Grand.Api.DTOs.Common;
using Grand.Api.Queries.Models.Common;
using Grand.Domain.Data;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Queries.Handlers.Common
{
    public class GetStoreQueryHandler : IRequestHandler<GetQuery<StoreDto>, IMongoQueryable<StoreDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetStoreQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }
        public Task<IMongoQueryable<StoreDto>> Handle(GetQuery<StoreDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult(_mongoDBContext.Database().GetCollection<StoreDto>(typeof(Domain.Stores.Store).Name).AsQueryable());
            else
                return Task.FromResult(_mongoDBContext.Database().GetCollection<StoreDto>(typeof(Domain.Stores.Store).Name).AsQueryable().Where(x => x.Id == request.Id));
        }
    }
}
