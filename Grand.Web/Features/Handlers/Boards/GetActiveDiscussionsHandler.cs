using Grand.Domain.Forums;
using Grand.Services.Forums;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetActiveDiscussionsHandler : IRequestHandler<GetActiveDiscussions, ActiveDiscussionsModel>
    {
        private readonly IForumService _forumService;
        private readonly IMediator _mediator;
        private readonly ForumSettings _forumSettings;

        public GetActiveDiscussionsHandler(
            IForumService forumService, 
            IMediator mediator, 
            ForumSettings forumSettings)
        {
            _forumService = forumService;
            _mediator = mediator;
            _forumSettings = forumSettings;
        }

        public async Task<ActiveDiscussionsModel> Handle(GetActiveDiscussions request, CancellationToken cancellationToken)
        {
            var topics = await _forumService.GetActiveTopics("", 0, _forumSettings.HomePageActiveDiscussionsTopicCount);
            if (!topics.Any())
                return null;
            var model = new ActiveDiscussionsModel();
            foreach (var topic in topics)
            {
                var topicModel = await _mediator.Send(new GetForumTopicRow() { Topic = topic });
                model.ForumTopics.Add(topicModel);
            }
            model.ViewAllLinkEnabled = true;
            model.ActiveDiscussionsFeedEnabled = _forumSettings.ActiveDiscussionsFeedEnabled;
            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.AllowPostVoting = _forumSettings.AllowPostVoting;

            return model;
        }
    }
}
