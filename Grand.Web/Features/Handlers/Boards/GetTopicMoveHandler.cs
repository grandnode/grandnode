using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetTopicMoveHandler : IRequestHandler<GetTopicMove, TopicMoveModel>
    {
        private readonly IMediator _mediator;
        public GetTopicMoveHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<TopicMoveModel> Handle(GetTopicMove request, CancellationToken cancellationToken)
        {
            var model = new TopicMoveModel();
            model.ForumList = await _mediator.Send(new GetGroupsForumsList());
            model.Id = request.ForumTopic.Id;
            model.TopicSeName = request.ForumTopic.GetSeName();
            model.ForumSelected = request.ForumTopic.ForumId;
            return model;
        }
    }
}
