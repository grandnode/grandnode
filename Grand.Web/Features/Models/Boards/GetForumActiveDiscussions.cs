using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetForumActiveDiscussions : IRequest<ActiveDiscussionsModel>
    {
        public string ForumId { get; set; } = "";
        public int PageNumber { get; set; } = 1;
    }
}
