using Grand.Core.Domain.Customers;
using Grand.Core.Domain.News;
using Grand.Core.Infrastructure;
using Grand.Framework.Extensions;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.News;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class NewsViewModelService : INewsViewModelService
    {
        #region Fields

        private readonly INewsService _newsService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;

        #endregion

        #region Constructors

        public NewsViewModelService(INewsService newsService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IStoreService storeService,
            IPictureService pictureService)
        {
            _newsService = newsService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _storeService = storeService;
            _pictureService = pictureService;
        }

        #endregion

        public virtual (IEnumerable<NewsItemModel> newsItemModels, int totalCount) PrepareNewsItemModel(NewsItemListModel model, int pageIndex, int pageSize)
        {
            var news = _newsService.GetAllNews(model.SearchStoreId, pageIndex - 1, pageSize, true, true);
            return (news.Select(x =>
            {
                var m = x.ToModel();
                m.Full = "";
                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                m.Comments = x.CommentCount;
                return m;
            }), news.TotalCount);
        }
        public virtual NewsItem InsertNewsItemModel(NewsItemModel model)
        {
            var newsItem = model.ToEntity();
            newsItem.CreatedOnUtc = DateTime.UtcNow;
            _newsService.InsertNews(newsItem);

            var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
            newsItem.SeName = seName;
            newsItem.Locales = model.Locales.ToLocalizedProperty(newsItem, x => x.Title, _urlRecordService);
            _newsService.UpdateNews(newsItem);
            //search engine name
            _urlRecordService.SaveSlug(newsItem, seName, "");

            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(newsItem.PictureId, newsItem.Title);
            return newsItem;
        }
        public virtual NewsItem UpdateNewsItemModel(NewsItem newsItem, NewsItemModel model)
        {
            string prevPictureId = newsItem.PictureId;
            newsItem = model.ToEntity(newsItem);
            var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
            newsItem.SeName = seName;
            newsItem.Locales = model.Locales.ToLocalizedProperty(newsItem, x => x.Title, _urlRecordService);
            _newsService.UpdateNews(newsItem);

            //search engine name
            _urlRecordService.SaveSlug(newsItem, seName, "");

            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != newsItem.PictureId)
            {
                var prevPicture = _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    _pictureService.DeletePicture(prevPicture);
            }

            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(newsItem.PictureId, newsItem.Title);
            return newsItem;
        }
        public virtual (IEnumerable<NewsCommentModel> newsCommentModels, int totalCount) PrepareNewsCommentModel(string filterByNewsItemId, int pageIndex, int pageSize)
        {
            IList<NewsComment> comments;
            if (!String.IsNullOrEmpty(filterByNewsItemId))
            {
                //filter comments by news item
                var newsItem = _newsService.GetNewsById(filterByNewsItemId);
                comments = newsItem.NewsComments.OrderBy(bc => bc.CreatedOnUtc).ToList();
            }
            else
            {
                //load all news comments
                comments = _newsService.GetAllComments("");
            }
            var customerService = EngineContext.Current.Resolve<ICustomerService>();
            return (comments.PagedForCommand(pageIndex, pageSize).Select(newsComment =>
                {
                    var commentModel = new NewsCommentModel();
                    commentModel.Id = newsComment.Id;
                    commentModel.NewsItemId = newsComment.NewsItemId;
                    commentModel.NewsItemTitle = _newsService.GetNewsById(newsComment.NewsItemId).Title;
                    commentModel.CustomerId = newsComment.CustomerId;
                    var customer = customerService.GetCustomerById(newsComment.CustomerId);
                    commentModel.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsComment.CreatedOnUtc, DateTimeKind.Utc);
                    commentModel.CommentTitle = newsComment.CommentTitle;
                    commentModel.CommentText = Core.Html.HtmlHelper.FormatText(newsComment.CommentText, false, true, false, false, false, false);
                    return commentModel;
                }), comments.Count);
        }
        public virtual void CommentDelete(NewsComment model)
        {
            var newsItem = _newsService.GetNewsById(model.NewsItemId);
            var comment = newsItem.NewsComments.FirstOrDefault(x => x.Id == model.Id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            newsItem.NewsComments.Remove(comment);
            //update totals
            newsItem.CommentCount = newsItem.NewsComments.Count;
            _newsService.UpdateNews(newsItem);
        }
    }
}
