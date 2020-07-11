using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetForumTopicPage : IRequest<ForumTopicPageModel>
    {
        public Customer Customer { get; set; }
        public ForumTopic ForumTopic { get; set; }
        public int PageNumber { get; set; }
    }
}
