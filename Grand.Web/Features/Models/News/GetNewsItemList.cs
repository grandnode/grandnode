using Grand.Web.Models.News;
using MediatR;

namespace Grand.Web.Features.Models.News
{
    public class GetNewsItemList : IRequest<NewsItemListModel>
    {
        public NewsPagingFilteringModel Command { get; set; }
    }
}
