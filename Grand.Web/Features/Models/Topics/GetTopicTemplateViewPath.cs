using MediatR;

namespace Grand.Web.Features.Models.Topics
{
    public class GetTopicTemplateViewPath : IRequest<string>
    {
        public string TemplateId { get; set; }
    }
}
