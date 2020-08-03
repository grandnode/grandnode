using Grand.Domain.Forums;
using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetTopicMove : IRequest<TopicMoveModel>
    {
        public ForumTopic ForumTopic { get; set; }
    }
}
