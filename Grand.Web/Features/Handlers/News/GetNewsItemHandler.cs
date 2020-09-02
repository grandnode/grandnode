using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Customers;
using Grand.Domain.Media;
using Grand.Domain.News;
using Grand.Framework.Security.Captcha;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Web.Features.Models.News;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Media;
using Grand.Web.Models.News;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.News
{
    public class GetNewsItemHandler : IRequestHandler<GetNewsItem, NewsItemModel>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;

        private readonly MediaSettings _mediaSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;

        public GetNewsItemHandler(ICacheManager cacheManager, IWorkContext workContext, IStoreContext storeContext, IDateTimeHelper dateTimeHelper,
            IPictureService pictureService, ILocalizationService localizationService, ICustomerService customerService,
            MediaSettings mediaSettings, CaptchaSettings captchaSettings, CustomerSettings customerSettings)
        {
            _cacheManager = cacheManager;
            _workContext = workContext;
            _storeContext = storeContext;
            _dateTimeHelper = dateTimeHelper;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _customerService = customerService;

            _mediaSettings = mediaSettings;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
        }

        public async Task<NewsItemModel> Handle(GetNewsItem request, CancellationToken cancellationToken)
        {
            var model = new NewsItemModel();
            model.Id = request.NewsItem.Id;
            model.MetaTitle = request.NewsItem.GetLocalized(x => x.MetaTitle, _workContext.WorkingLanguage.Id);
            model.MetaDescription = request.NewsItem.GetLocalized(x => x.MetaDescription, _workContext.WorkingLanguage.Id);
            model.MetaKeywords = request.NewsItem.GetLocalized(x => x.MetaKeywords, _workContext.WorkingLanguage.Id);
            model.SeName = request.NewsItem.GetSeName(_workContext.WorkingLanguage.Id);
            model.Title = request.NewsItem.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id);
            model.Short = request.NewsItem.GetLocalized(x => x.Short, _workContext.WorkingLanguage.Id);
            model.Full = request.NewsItem.GetLocalized(x => x.Full, _workContext.WorkingLanguage.Id);
            model.AllowComments = request.NewsItem.AllowComments;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(request.NewsItem.StartDateUtc ?? request.NewsItem.CreatedOnUtc, DateTimeKind.Utc);
            model.NumberOfComments = request.NewsItem.CommentCount;
            model.AddNewComment.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnNewsCommentPage;

            //prepare comments
            await PrepareComments(request.NewsItem, model);

            //prepare picture model
            await PreparePicture(request.NewsItem, model);

            return model;
        }

        private async Task PrepareComments(NewsItem newsItem, NewsItemModel model)
        {
            var newsComments = newsItem.NewsComments.OrderBy(pr => pr.CreatedOnUtc);
            foreach (var nc in newsComments)
            {
                var customer = await _customerService.GetCustomerById(nc.CustomerId);
                var commentModel = new NewsCommentModel {
                    Id = nc.Id,
                    CustomerId = nc.CustomerId,
                    CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                    CommentTitle = nc.CommentTitle,
                    CommentText = nc.CommentText,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(nc.CreatedOnUtc, DateTimeKind.Utc),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                };
                if (_customerSettings.AllowCustomersToUploadAvatars)
                {
                    commentModel.CustomerAvatarUrl = await _pictureService.GetPictureUrl(
                        customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId),
                        _mediaSettings.AvatarPictureSize,
                        _customerSettings.DefaultAvatarEnabled,
                        defaultPictureType: PictureType.Avatar);
                }
                model.Comments.Add(commentModel);
            }
        }

        private async Task PreparePicture(NewsItem newsItem, NewsItemModel model)
        {
            if (!string.IsNullOrEmpty(newsItem.PictureId))
            {
                var categoryPictureCacheKey = string.Format(ModelCacheEventConst.NEWS_PICTURE_MODEL_KEY, newsItem.Id, _mediaSettings.NewsThumbPictureSize,
                    true, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
                model.PictureModel = await _cacheManager.GetAsync(categoryPictureCacheKey, async () =>
                {
                    var pictureModel = new PictureModel {
                        Id = newsItem.PictureId,
                        FullSizeImageUrl = await _pictureService.GetPictureUrl(newsItem.PictureId),
                        ImageUrl = await _pictureService.GetPictureUrl(newsItem.PictureId, _mediaSettings.NewsThumbPictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.News.ImageLinkTitleFormat"), newsItem.Title),
                        AlternateText = string.Format(_localizationService.GetResource("Media.News.ImageAlternateTextFormat"), newsItem.Title)
                    };
                    return pictureModel;
                });
            }
        }

    }
}
