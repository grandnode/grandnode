using Grand.Web.Models.Topics;
using MediatR;

namespace Grand.Web.Features.Models.Topics
{
    public class GetTopicBlock : IRequest<TopicModel>
    {
        public string SystemName { get; set; }
        public string TopicId { get; set; }
    }
}
