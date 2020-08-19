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
    public class GetCurrencyQueryHandler : IRequestHandler<GetQuery<CurrencyDto>, IMongoQueryable<CurrencyDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetCurrencyQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }
        public Task<IMongoQueryable<CurrencyDto>> Handle(GetQuery<CurrencyDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult(_mongoDBContext.Database().GetCollection<CurrencyDto>(typeof(Domain.Directory.Currency).Name).AsQueryable());
            else
                return Task.FromResult(_mongoDBContext.Database().GetCollection<CurrencyDto>(typeof(Domain.Directory.Currency).Name).AsQueryable().Where(x => x.Id == request.Id));
        }
    }
}
