using Grand.Core.Domain.News;
using Grand.Web.Models.News;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface INewsViewModelService
    {
        Task PrepareNewsItemModel(NewsItemModel model, NewsItem newsItem, bool prepareComments);
        Task<HomePageNewsItemsModel> PrepareHomePageNewsItems();
        Task<NewsItemListModel> PrepareNewsItemList(NewsPagingFilteringModel command);

        Task InsertNewsComment(NewsItem newsItem, NewsItemModel model);

    }
}