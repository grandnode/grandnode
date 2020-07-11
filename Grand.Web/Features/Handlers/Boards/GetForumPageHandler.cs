using Grand.Domain.Forums;
using Grand.Services.Forums;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetForumPageHandler : IRequestHandler<GetForumPage, ForumPageModel>
    {
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;
        private readonly IMediator _mediator;
        private readonly ForumSettings _forumSettings;

        public GetForumPageHandler(
            IForumService forumService,
            ILocalizationService localizationService,
            IMediator mediator,
            ForumSettings forumSettings)
        {
            _forumService = forumService;
            _localizationService = localizationService;
            _mediator = mediator;
            _forumSettings = forumSettings;
        }

        public async Task<ForumPageModel> Handle(GetForumPage request, CancellationToken cancellationToken)
        {
            var model = new ForumPageModel {
                Id = request.Forum.Id,
                Name = request.Forum.Name,
                SeName = request.Forum.GetSeName(),
                Description = request.Forum.Description
            };

            int pageSize = _forumSettings.TopicsPageSize > 0 ? _forumSettings.TopicsPageSize : 10;

            //subscription                
            if (_forumService.IsCustomerAllowedToSubscribe(request.Customer))
            {
                model.WatchForumText = _localizationService.GetResource("Forum.WatchForum");

                var forumSubscription = (await _forumService.GetAllSubscriptions(request.Customer.Id, request.Forum.Id, "", 0, 1)).FirstOrDefault();
                if (forumSubscription != null)
                {
                    model.WatchForumText = _localizationService.GetResource("Forum.UnwatchForum");
                }
            }

            var topics = await _forumService.GetAllTopics(request.Forum.Id, "", string.Empty,
                ForumSearchType.All, 0, (request.PageNumber - 1), pageSize);
            model.TopicPageSize = topics.PageSize;
            model.TopicTotalRecords = topics.TotalCount;
            model.TopicPageIndex = topics.PageIndex;
            foreach (var topic in topics)
            {
                var topicModel = await _mediator.Send(new GetForumTopicRow() { Topic = topic });
                model.ForumTopics.Add(topicModel);
            }
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(request.Customer);
            model.ForumFeedsEnabled = _forumSettings.ForumFeedsEnabled;
            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.AllowPostVoting = _forumSettings.AllowPostVoting;
            return model;
        }
    }
}
