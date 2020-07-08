using Grand.Domain.News;
using Grand.Web.Models.News;
using MediatR;

namespace Grand.Web.Features.Models.News
{
    public class GetNewsItem : IRequest<NewsItemModel>
    {
        public NewsItem NewsItem { get; set; }
    }
}
