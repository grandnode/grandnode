using Grand.Core.Domain.News;
using Grand.Web.Areas.Admin.Models.News;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface INewsViewModelService
    {
        (IEnumerable<NewsItemModel> newsItemModels, int totalCount) PrepareNewsItemModel(NewsItemListModel model, int pageIndex, int pageSize);
        NewsItem InsertNewsItemModel(NewsItemModel model);
        NewsItem UpdateNewsItemModel(NewsItem newsItem, NewsItemModel model);
        (IEnumerable<NewsCommentModel> newsCommentModels, int totalCount) PrepareNewsCommentModel(string filterByNewsItemId, int pageIndex, int pageSize);
        void CommentDelete(NewsComment model);
    }
}
