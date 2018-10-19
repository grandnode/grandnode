using Grand.Core.Domain.News;
using Grand.Web.Models.News;

namespace Grand.Web.Services
{
    public partial interface INewsViewModelService
    {
        void PrepareNewsItemModel(NewsItemModel model, NewsItem newsItem, bool prepareComments);
        HomePageNewsItemsModel PrepareHomePageNewsItems();
        NewsItemListModel PrepareNewsItemList(NewsPagingFilteringModel command);

        void InsertNewsComment(NewsItem newsItem, NewsItemModel model);

    }
}