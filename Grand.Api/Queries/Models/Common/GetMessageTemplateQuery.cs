using Grand.Api.DTOs.Common;
using MediatR;
using MongoDB.Driver.Linq;

namespace Grand.Api.Queries.Models.Common
{
    public class GetMessageTemplateQuery : IRequest<IMongoQueryable<MessageTemplateDto>>
    {
        public string Id { get; set; }
        public string TemplateName { get; set; }
    }
}
