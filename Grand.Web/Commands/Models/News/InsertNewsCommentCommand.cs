using Grand.Domain.News;
using Grand.Web.Models.News;
using MediatR;

namespace Grand.Web.Commands.Models.News
{
    public class InsertNewsCommentCommand : IRequest<NewsComment>
    {
        public NewsItem NewsItem { get; set; }
        public NewsItemModel Model { get; set; }
    }
}
