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
    public class GetMessageTemplateQueryHandler : IRequestHandler<GetMessageTemplateQuery, IMongoQueryable<MessageTemplateDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetMessageTemplateQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }

        public Task<IMongoQueryable<MessageTemplateDto>> Handle(GetMessageTemplateQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult(_mongoDBContext.Database().GetCollection<MessageTemplateDto>(request.TemplateName).AsQueryable());
            else
                return Task.FromResult(_mongoDBContext.Database().GetCollection<MessageTemplateDto>(request.TemplateName).AsQueryable().Where(x => x.Id == request.Id));
        }
    }
}
