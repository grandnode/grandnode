using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Media;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetForumTopicPageHandler : IRequestHandler<GetForumTopicPage, ForumTopicPageModel>
    {
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPictureService _pictureService;
        private readonly ICountryService _countryService;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;

        public GetForumTopicPageHandler(
            IForumService forumService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IPictureService pictureService,
            ICountryService countryService,
            ForumSettings forumSettings,
            CustomerSettings customerSettings,
            MediaSettings mediaSettings)
        {
            _forumService = forumService;
            _localizationService = localizationService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _pictureService = pictureService;
            _countryService = countryService;
            _forumSettings = forumSettings;
            _customerSettings = customerSettings;
            _mediaSettings = mediaSettings;
        }

        public async Task<ForumTopicPageModel> Handle(GetForumTopicPage request, CancellationToken cancellationToken)
        {
            var posts = await _forumService.GetAllPosts(request.ForumTopic.Id, "", string.Empty,
                request.PageNumber - 1, _forumSettings.PostsPageSize);

            //prepare model
            var model = new ForumTopicPageModel();
            model.Id = request.ForumTopic.Id;
            model.Subject = request.ForumTopic.Subject;
            model.SeName = request.ForumTopic.GetSeName();
            model.IsCustomerAllowedToEditTopic = _forumService.IsCustomerAllowedToEditTopic(request.Customer, request.ForumTopic);
            model.IsCustomerAllowedToDeleteTopic = _forumService.IsCustomerAllowedToDeleteTopic(request.Customer, request.ForumTopic);
            model.IsCustomerAllowedToMoveTopic = _forumService.IsCustomerAllowedToMoveTopic(request.Customer, request.ForumTopic);
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(request.Customer);

            if (model.IsCustomerAllowedToSubscribe)
            {
                model.WatchTopicText = _localizationService.GetResource("Forum.WatchTopic");

                var forumTopicSubscription = (await _forumService.GetAllSubscriptions(request.Customer.Id,
                    "", request.ForumTopic.Id, 0, 1)).FirstOrDefault();
                if (forumTopicSubscription != null)
                {
                    model.WatchTopicText = _localizationService.GetResource("Forum.UnwatchTopic");
                }
            }
            model.PostsPageIndex = posts.PageIndex;
            model.PostsPageSize = posts.PageSize;
            model.PostsTotalRecords = posts.TotalCount;
            foreach (var post in posts)
            {
                var customer = await _customerService.GetCustomerById(post.CustomerId);
                var forumPostModel = new ForumPostModel {
                    Id = post.Id,
                    ForumTopicId = post.TopicId,
                    ForumTopicSeName = request.ForumTopic.GetSeName(),
                    FormattedText = post.FormatPostText(),
                    IsCurrentCustomerAllowedToEditPost = _forumService.IsCustomerAllowedToEditPost(request.Customer, post),
                    IsCurrentCustomerAllowedToDeletePost = _forumService.IsCustomerAllowedToDeletePost(request.Customer, post),
                    CustomerId = post.CustomerId,
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles,
                    CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                    IsCustomerForumModerator = customer.IsForumModerator(),
                    IsCustomerGuest = customer.IsGuest(),
                    ShowCustomersPostCount = _forumSettings.ShowCustomersPostCount,
                    ForumPostCount = customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.ForumPostCount),
                    ShowCustomersJoinDate = _customerSettings.ShowCustomersJoinDate,
                    CustomerJoinDate = customer.CreatedOnUtc,
                    AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                    SignaturesEnabled = _forumSettings.SignaturesEnabled,
                    FormattedSignature = (customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Signature)).FormatForumSignatureText(),
                };
                //created on string
                if (_forumSettings.RelativeDateTimeFormattingEnabled)
                    forumPostModel.PostCreatedOnStr = post.CreatedOnUtc.ToString("f");
                else
                    forumPostModel.PostCreatedOnStr = _dateTimeHelper.ConvertToUserTime(post.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
                //avatar
                if (_customerSettings.AllowCustomersToUploadAvatars)
                {
                    forumPostModel.CustomerAvatarUrl = await _pictureService.GetPictureUrl(
                        request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId),
                        _mediaSettings.AvatarPictureSize,
                        _customerSettings.DefaultAvatarEnabled,
                        defaultPictureType: PictureType.Avatar);
                }
                //location
                forumPostModel.ShowCustomersLocation = _customerSettings.ShowCustomersLocation;
                if (_customerSettings.ShowCustomersLocation)
                {
                    var countryId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId);
                    var country = await _countryService.GetCountryById(countryId);
                    forumPostModel.CustomerLocation = country != null ? country.Name : string.Empty;
                }

                if (_forumSettings.AllowPostVoting)
                {
                    forumPostModel.AllowPostVoting = true;
                    forumPostModel.VoteCount = post.VoteCount;
                    var postVote = await _forumService.GetPostVote(post.Id, customer.Id);
                    if (postVote != null)
                        forumPostModel.VoteIsUp = postVote.IsUp;
                }
                // page number is needed for creating post link in _ForumPost partial view
                forumPostModel.CurrentTopicPage = request.PageNumber;
                model.ForumPostModels.Add(forumPostModel);
            }

            return model;
        }
    }
}
