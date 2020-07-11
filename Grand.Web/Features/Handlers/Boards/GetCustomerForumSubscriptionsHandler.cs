using Grand.Domain.Forums;
using Grand.Services.Forums;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using Grand.Web.Models.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetCustomerForumSubscriptionsHandler : IRequestHandler<GetCustomerForumSubscriptions, CustomerForumSubscriptionsModel>
    {
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;

        private readonly ForumSettings _forumSettings;

        public GetCustomerForumSubscriptionsHandler(
            IForumService forumService,
            ILocalizationService localizationService,
            ForumSettings forumSettings)
        {
            _forumService = forumService;
            _localizationService = localizationService;
            _forumSettings = forumSettings;
        }

        public async Task<CustomerForumSubscriptionsModel> Handle(GetCustomerForumSubscriptions request, CancellationToken cancellationToken)
        {
            var pageSize = _forumSettings.ForumSubscriptionsPageSize;
            var list = await _forumService.GetAllSubscriptions(request.Customer.Id, "", "", request.PageIndex, pageSize);
            var model = new CustomerForumSubscriptionsModel();
            foreach (var forumSubscription in list)
            {
                var forumTopicId = forumSubscription.TopicId;
                var forumId = forumSubscription.ForumId;
                bool topicSubscription = false;
                var title = string.Empty;
                var slug = string.Empty;

                if (!String.IsNullOrEmpty(forumTopicId))
                {
                    topicSubscription = true;
                    var forumTopic = await _forumService.GetTopicById(forumTopicId);
                    if (forumTopic != null)
                    {
                        title = forumTopic.Subject;
                        slug = forumTopic.GetSeName();
                    }
                }
                else
                {
                    var forum = await _forumService.GetForumById(forumId);
                    if (forum != null)
                    {
                        title = forum.Name;
                        slug = forum.GetSeName();
                    }
                }

                model.ForumSubscriptions.Add(new CustomerForumSubscriptionsModel.ForumSubscriptionModel {
                    Id = forumSubscription.Id,
                    ForumTopicId = forumTopicId,
                    ForumId = forumSubscription.ForumId,
                    TopicSubscription = topicSubscription,
                    Title = title,
                    Slug = slug,
                });
            }

            model.PagerModel = new PagerModel(_localizationService) {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerForumSubscriptionsPaged",
                UseRouteLinks = true,
                RouteValues = new ForumSubscriptionsRouteValues { pageNumber = request.PageIndex }
            };

            return model;
        }
    }
}
