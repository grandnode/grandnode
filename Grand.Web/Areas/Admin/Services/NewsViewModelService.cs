using Grand.Domain.Customers;
using Grand.Domain.News;
using Grand.Domain.Seo;
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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class NewsViewModelService : INewsViewModelService
    {
        #region Fields

        private readonly INewsService _newsService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region Constructors

        public NewsViewModelService(INewsService newsService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            IServiceProvider serviceProvider)
        {
            _newsService = newsService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _serviceProvider = serviceProvider;
        }

        #endregion

        public virtual async Task<(IEnumerable<NewsItemModel> newsItemModels, int totalCount)> PrepareNewsItemModel(NewsItemListModel model, int pageIndex, int pageSize)
        {
            var news = await _newsService.GetAllNews(model.SearchStoreId, pageIndex - 1, pageSize, true, true);
            return (news.Select(x =>
            {
                var m = x.ToModel(_dateTimeHelper);
                m.Full = "";
                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                m.Comments = x.CommentCount;
                return m;
            }), news.TotalCount);
        }
        public virtual async Task<NewsItem> InsertNewsItemModel(NewsItemModel model)
        {
            var datetimeHelper = _serviceProvider.GetRequiredService<IDateTimeHelper>();
            var newsItem = model.ToEntity(_dateTimeHelper);
            newsItem.CreatedOnUtc = DateTime.UtcNow;
            await _newsService.InsertNews(newsItem);

            var seName = await newsItem.ValidateSeName(model.SeName, model.Title, true, _serviceProvider.GetRequiredService<SeoSettings>(), _urlRecordService, _serviceProvider.GetRequiredService<ILanguageService>());
            newsItem.SeName = seName;
            newsItem.Locales = await model.Locales.ToLocalizedProperty(newsItem, x => x.Title, _serviceProvider.GetRequiredService<SeoSettings>(), _urlRecordService, _serviceProvider.GetRequiredService<ILanguageService>());
            await _newsService.UpdateNews(newsItem);
            //search engine name
            await _urlRecordService.SaveSlug(newsItem, seName, "");

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(newsItem.PictureId, newsItem.Title);
            return newsItem;
        }
        public virtual async Task<NewsItem> UpdateNewsItemModel(NewsItem newsItem, NewsItemModel model)
        {
            string prevPictureId = newsItem.PictureId;
            newsItem = model.ToEntity(newsItem, _dateTimeHelper);
            var seName = await newsItem.ValidateSeName(model.SeName, model.Title, true, _serviceProvider.GetRequiredService<SeoSettings>(), _urlRecordService, _serviceProvider.GetRequiredService<ILanguageService>());
            newsItem.SeName = seName;
            newsItem.Locales = await model.Locales.ToLocalizedProperty(newsItem, x => x.Title, _serviceProvider.GetRequiredService<SeoSettings>(), _urlRecordService, _serviceProvider.GetRequiredService<ILanguageService>());
            await _newsService.UpdateNews(newsItem);

            //search engine name
            await _urlRecordService.SaveSlug(newsItem, seName, "");

            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != newsItem.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(newsItem.PictureId, newsItem.Title);
            return newsItem;
        }
        public virtual async Task<(IEnumerable<NewsCommentModel> newsCommentModels, int totalCount)> PrepareNewsCommentModel(string filterByNewsItemId, int pageIndex, int pageSize)
        {
            IList<NewsComment> comments;
            if (!String.IsNullOrEmpty(filterByNewsItemId))
            {
                //filter comments by news item
                var newsItem = await _newsService.GetNewsById(filterByNewsItemId);
                comments = newsItem.NewsComments.OrderBy(bc => bc.CreatedOnUtc).ToList();
            }
            else
            {
                //load all news comments
                comments = await _newsService.GetAllComments("");
            }
            var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
            var items = new List<NewsCommentModel>();
            foreach (var newsComment in comments.PagedForCommand(pageIndex, pageSize))
            {
                var commentModel = new NewsCommentModel
                {
                    Id = newsComment.Id,
                    NewsItemId = newsComment.NewsItemId,
                    NewsItemTitle = (await _newsService.GetNewsById(newsComment.NewsItemId))?.Title,
                    CustomerId = newsComment.CustomerId
                };
                var customer = await customerService.GetCustomerById(newsComment.CustomerId);
                commentModel.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsComment.CreatedOnUtc, DateTimeKind.Utc);
                commentModel.CommentTitle = newsComment.CommentTitle;
                commentModel.CommentText = Core.Html.HtmlHelper.FormatText(newsComment.CommentText, false, true, false, false, false, false);
                items.Add(commentModel);
            }
            return (items, comments.Count);
        }
        public virtual async Task CommentDelete(NewsComment model)
        {
            var newsItem = await _newsService.GetNewsById(model.NewsItemId);
            var comment = newsItem.NewsComments.FirstOrDefault(x => x.Id == model.Id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            newsItem.NewsComments.Remove(comment);
            //update totals
            newsItem.CommentCount = newsItem.NewsComments.Count;
            await _newsService.UpdateNews(newsItem);
        }
    }
}
