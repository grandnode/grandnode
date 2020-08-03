using Grand.Domain.News;
using Grand.Web.Areas.Admin.Models.News;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface INewsViewModelService
    {
        Task<(IEnumerable<NewsItemModel> newsItemModels, int totalCount)> PrepareNewsItemModel(NewsItemListModel model, int pageIndex, int pageSize);
        Task<NewsItem> InsertNewsItemModel(NewsItemModel model);
        Task<NewsItem> UpdateNewsItemModel(NewsItem newsItem, NewsItemModel model);
        Task<(IEnumerable<NewsCommentModel> newsCommentModels, int totalCount)> PrepareNewsCommentModel(string filterByNewsItemId, int pageIndex, int pageSize);
        Task CommentDelete(NewsComment model);
    }
}
