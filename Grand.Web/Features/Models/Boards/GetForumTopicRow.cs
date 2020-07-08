using Grand.Domain.Forums;
using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetForumTopicRow : IRequest<ForumTopicRowModel>
    {
        public ForumTopic Topic { get; set; }
    }
}
