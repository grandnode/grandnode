using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetForumTopicRowHandler : IRequestHandler<GetForumTopicRow, ForumTopicRowModel>
    {
        private readonly ICustomerService _customerService;
        private readonly IForumService _forumService;
        private readonly CustomerSettings _customerSettings;
        private readonly ForumSettings _forumSettings;

        public GetForumTopicRowHandler(
            ICustomerService customerService,
            IForumService forumService,
            CustomerSettings customerSettings,
            ForumSettings forumSettings)
        {
            _customerService = customerService;
            _forumService = forumService;
            _customerSettings = customerSettings;
            _forumSettings = forumSettings;
        }

        public async Task<ForumTopicRowModel> Handle(GetForumTopicRow request, CancellationToken cancellationToken)
        {
            var customer = await _customerService.GetCustomerById(request.Topic.CustomerId);
            var topicModel = new ForumTopicRowModel {
                Id = request.Topic.Id,
                Subject = request.Topic.Subject,
                SeName = request.Topic.GetSeName(),
                LastPostId = request.Topic.LastPostId,
                NumPosts = request.Topic.NumPosts,
                Views = request.Topic.Views,
                NumReplies = request.Topic.NumReplies,
                ForumTopicType = request.Topic.ForumTopicType,
                CustomerId = request.Topic.CustomerId,
                AllowViewingProfiles = _customerSettings.AllowViewingProfiles,
                CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                IsCustomerGuest = customer.IsGuest()
            };

            var forumPosts = await _forumService.GetAllPosts(request.Topic.Id, "", string.Empty, 1, _forumSettings.PostsPageSize);
            topicModel.TotalPostPages = forumPosts.TotalPages;

            var firstPost = await request.Topic.GetFirstPost(_forumService);
            topicModel.Votes = firstPost != null ? firstPost.VoteCount : 0;

            return topicModel;
        }
    }
}
