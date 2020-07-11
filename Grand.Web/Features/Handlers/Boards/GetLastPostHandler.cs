using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetLastPostHandler : IRequestHandler<GetLastPost, LastPostModel>
    {
        private readonly IForumService _forumService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;

        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;

        public GetLastPostHandler(
            IForumService forumService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ForumSettings forumSettings,
            CustomerSettings customerSettings)
        {
            _forumService = forumService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _forumSettings = forumSettings;
            _customerSettings = customerSettings;
        }

        public async Task<LastPostModel> Handle(GetLastPost request, CancellationToken cancellationToken)
        {
            var post = await _forumService.GetPostById(request.ForumPostId);
            var model = new LastPostModel();
            if (post != null)
            {
                var forumTopic = await _forumService.GetTopicById(post.TopicId);
                if (forumTopic != null)
                {
                    var customer = await _customerService.GetCustomerById(post.CustomerId);
                    model.Id = post.Id;
                    model.ForumTopicId = post.TopicId;
                    model.ForumTopicSeName = forumTopic.GetSeName();
                    model.ForumTopicSubject = forumTopic.StripTopicSubject(_forumSettings);
                    model.CustomerId = post.CustomerId;
                    model.AllowViewingProfiles = _customerSettings.AllowViewingProfiles;
                    model.CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat);
                    model.IsCustomerGuest = customer.IsGuest();
                    //created on string
                    if (_forumSettings.RelativeDateTimeFormattingEnabled)
                        model.PostCreatedOnStr = post.CreatedOnUtc.ToString("f");
                    else
                        model.PostCreatedOnStr = _dateTimeHelper.ConvertToUserTime(post.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
                }
            }
            model.ShowTopic = request.ShowTopic;
            return model;
        }
    }
}
