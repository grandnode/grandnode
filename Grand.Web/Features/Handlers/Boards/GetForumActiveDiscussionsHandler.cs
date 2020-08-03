using Grand.Domain.Forums;
using Grand.Services.Forums;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetForumActiveDiscussionsHandler : IRequestHandler<GetForumActiveDiscussions, ActiveDiscussionsModel>
    {
        private readonly IMediator _mediator;
        private readonly IForumService _forumService;
        private readonly ForumSettings _forumSettings;

        public GetForumActiveDiscussionsHandler(
            IMediator mediator,
            IForumService forumService,
            ForumSettings forumSettings)
        {
            _mediator = mediator;
            _forumService = forumService;
            _forumSettings = forumSettings;
        }

        public async Task<ActiveDiscussionsModel> Handle(GetForumActiveDiscussions request, CancellationToken cancellationToken)
        {
            var model = new ActiveDiscussionsModel();

            var pageSize = _forumSettings.ActiveDiscussionsPageSize > 0 ? _forumSettings.ActiveDiscussionsPageSize : 50;

            var topics = await _forumService.GetActiveTopics(request.ForumId, (request.PageNumber - 1), pageSize);
            model.TopicPageSize = topics.PageSize;
            model.TopicTotalRecords = topics.TotalCount;
            model.TopicPageIndex = topics.PageIndex;
            foreach (var topic in topics)
            {
                var topicModel = await _mediator.Send(new GetForumTopicRow() { Topic = topic });
                model.ForumTopics.Add(topicModel);
            }
            model.ViewAllLinkEnabled = false;
            model.ActiveDiscussionsFeedEnabled = _forumSettings.ActiveDiscussionsFeedEnabled;
            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.AllowPostVoting = _forumSettings.AllowPostVoting;
            return model;
        }
    }
}
