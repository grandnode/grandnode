using Grand.Domain.Forums;
using Grand.Services.Forums;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetEditForumTopicHandler : IRequestHandler<GetEditForumTopic, EditForumTopicModel>
    {
        private readonly IForumService _forumService;
        private readonly IMediator _mediator;
        private readonly ForumSettings _forumSettings;

        public GetEditForumTopicHandler(
            IForumService forumService,
            IMediator mediator,
            ForumSettings forumSettings)
        {
            _forumService = forumService;
            _mediator = mediator;
            _forumSettings = forumSettings;
        }

        public async Task<EditForumTopicModel> Handle(GetEditForumTopic request, CancellationToken cancellationToken)
        {
            var model = new EditForumTopicModel();
            model.Id = "";
            model.IsEdit = false;
            model.ForumId = request.Forum.Id;
            model.ForumName = request.Forum.Name;
            model.ForumSeName = request.Forum.GetSeName();
            model.ForumEditor = _forumSettings.ForumEditor;
            model.IsCustomerAllowedToSetTopicPriority = _forumService.IsCustomerAllowedToSetTopicPriority(request.Customer);
            model.TopicPriorities = await _mediator.Send(new GetTopicTypesList());
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(request.Customer);
            model.Subscribed = false;
            return model;
        }
    }
}
