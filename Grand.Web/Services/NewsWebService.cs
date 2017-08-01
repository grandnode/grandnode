using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.News;
using Grand.Core.Infrastructure;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.News;
using Grand.Services.Seo;
using Grand.Framework.Security.Captcha;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.News;
using System;
using System.Linq;

namespace Grand.Web.Services
{
    public partial class NewsWebService: INewsWebService
    {

        private readonly INewsService _newsService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkflowMessageService _workflowMessageService;

        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly NewsSettings _newsSettings;
        private readonly LocalizationSettings _localizationSettings;

        public NewsWebService(INewsService newsService, IWorkContext workContext, IStoreContext storeContext,
            IPictureService pictureService, IDateTimeHelper dateTimeHelper, ICacheManager cacheManager,
            IWorkflowMessageService workflowMessageService,
            CaptchaSettings captchaSettings, NewsSettings newsSettings,
            CustomerSettings customerSettings, MediaSettings mediaSettings, LocalizationSettings localizationSettings)
        {
            this._newsService = newsService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._pictureService = pictureService;
            this._dateTimeHelper = dateTimeHelper;
            this._cacheManager = cacheManager;
            this._workflowMessageService = workflowMessageService;

            this._captchaSettings = captchaSettings;
            this._newsSettings = newsSettings;
            this._customerSettings = customerSettings;
            this._mediaSettings = mediaSettings;
            this._localizationSettings = localizationSettings;
        }

        public virtual void PrepareNewsItemModel(NewsItemModel model, NewsItem newsItem, bool prepareComments)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = newsItem.Id;
            model.MetaTitle = newsItem.GetLocalized(x => x.MetaTitle);
            model.MetaDescription = newsItem.GetLocalized(x => x.MetaDescription);
            model.MetaKeywords = newsItem.GetLocalized(x => x.MetaKeywords);
            model.SeName = newsItem.GetSeName();
            model.Title = newsItem.GetLocalized(x => x.Title);
            model.Short = newsItem.GetLocalized(x => x.Short);
            model.Full = newsItem.GetLocalized(x => x.Full);
            model.AllowComments = newsItem.AllowComments;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.StartDateUtc ?? newsItem.CreatedOnUtc, DateTimeKind.Utc);
            model.NumberOfComments = newsItem.CommentCount;
            model.AddNewComment.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnNewsCommentPage;
            if (prepareComments)
            {
                var newsComments = newsItem.NewsComments.OrderBy(pr => pr.CreatedOnUtc);
                foreach (var nc in newsComments)
                {
                    var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(nc.CustomerId);
                    var commentModel = new NewsCommentModel
                    {
                        Id = nc.Id,
                        CustomerId = nc.CustomerId,
                        CustomerName = customer.FormatUserName(),
                        CommentTitle = nc.CommentTitle,
                        CommentText = nc.CommentText,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(nc.CreatedOnUtc, DateTimeKind.Utc),
                        AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                    };
                    if (_customerSettings.AllowCustomersToUploadAvatars)
                    {
                        commentModel.CustomerAvatarUrl = _pictureService.GetPictureUrl(
                            customer.GetAttribute<string>(SystemCustomerAttributeNames.AvatarPictureId),
                            false,
                            _mediaSettings.AvatarPictureSize,
                            _customerSettings.DefaultAvatarEnabled,
                            defaultPictureType: PictureType.Avatar);
                    }
                    model.Comments.Add(commentModel);
                }
            }

        }
        public virtual HomePageNewsItemsModel PrepareHomePageNewsItems()
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.HOMEPAGE_NEWSMODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var newsItems = _newsService.GetAllNews(_storeContext.CurrentStore.Id, 0, _newsSettings.MainPageNewsCount);
                return new HomePageNewsItemsModel
                {
                    WorkingLanguageId = _workContext.WorkingLanguage.Id,
                    NewsItems = newsItems
                        .Select(x =>
                        {
                            var newsModel = new NewsItemModel();
                            PrepareNewsItemModel(newsModel, x, false);
                            return newsModel;
                        })
                        .ToList()
                };
            });

            //"Comments" property of "NewsItemModel" object depends on the current customer.
            //Furthermore, we just don't need it for home page news. So let's reset it.
            //But first we need to clone the cached model (the updated one should not be cached)
            var model = (HomePageNewsItemsModel)cachedModel.Clone();
            foreach (var newsItemModel in model.NewsItems)
                newsItemModel.Comments.Clear();

            return model;
        }
        public virtual NewsItemListModel PrepareNewsItemList(NewsPagingFilteringModel command)
        {
            var model = new NewsItemListModel();
            model.WorkingLanguageId = _workContext.WorkingLanguage.Id;

            if (command.PageSize <= 0) command.PageSize = _newsSettings.NewsArchivePageSize;
            if (command.PageNumber <= 0) command.PageNumber = 1;

            var newsItems = _newsService.GetAllNews(_storeContext.CurrentStore.Id,
                command.PageNumber - 1, command.PageSize);
            model.PagingFilteringContext.LoadPagedList(newsItems);

            model.NewsItems = newsItems
                .Select(x =>
                {
                    var newsModel = new NewsItemModel();
                    PrepareNewsItemModel(newsModel, x, false);
                    return newsModel;
                })
                .ToList();

            return model;
        }
        public virtual void InsertNewsComment(NewsItem newsItem, NewsItemModel model)
        {
            var comment = new NewsComment
            {
                NewsItemId = newsItem.Id,
                CustomerId = _workContext.CurrentCustomer.Id,
                CommentTitle = model.AddNewComment.CommentTitle,
                CommentText = model.AddNewComment.CommentText,
                CreatedOnUtc = DateTime.UtcNow,
            };
            newsItem.NewsComments.Add(comment);
            //update totals
            newsItem.CommentCount = newsItem.NewsComments.Count;
            _newsService.UpdateNews(newsItem);
            EngineContext.Current.Resolve<ICustomerService>().UpdateNewsItem(_workContext.CurrentCustomer);

            //notify a store owner;
            if (_newsSettings.NotifyAboutNewNewsComments)
                _workflowMessageService.SendNewsCommentNotificationMessage(comment, _localizationSettings.DefaultAdminLanguageId);

        }

    }
}