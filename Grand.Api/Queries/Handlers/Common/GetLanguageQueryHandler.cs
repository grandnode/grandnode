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
    public class GetLanguageQueryHandler : IRequestHandler<GetQuery<LanguageDto>, IMongoQueryable<LanguageDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetLanguageQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }
        public Task<IMongoQueryable<LanguageDto>> Handle(GetQuery<LanguageDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult(_mongoDBContext.Database().GetCollection<LanguageDto>(typeof(Domain.Localization.Language).Name).AsQueryable());
            else
                return Task.FromResult(_mongoDBContext.Database().GetCollection<LanguageDto>(typeof(Domain.Localization.Language).Name).AsQueryable().Where(x => x.Id == request.Id));
        }
    }
}
