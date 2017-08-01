using Grand.Core.Domain.News;
using Grand.Web.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial interface INewsWebService
    {
        void PrepareNewsItemModel(NewsItemModel model, NewsItem newsItem, bool prepareComments);
        HomePageNewsItemsModel PrepareHomePageNewsItems();
        NewsItemListModel PrepareNewsItemList(NewsPagingFilteringModel command);

        void InsertNewsComment(NewsItem newsItem, NewsItemModel model);

    }
}