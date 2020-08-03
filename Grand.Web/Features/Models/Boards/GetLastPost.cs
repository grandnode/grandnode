using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetLastPost : IRequest<LastPostModel>
    {
        public string ForumPostId { get; set; }
        public bool ShowTopic { get; set; }
    }
}
