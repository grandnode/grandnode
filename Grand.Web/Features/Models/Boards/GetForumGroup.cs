using Grand.Domain.Forums;
using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetForumGroup : IRequest<ForumGroupModel>
    {
        public ForumGroup ForumGroup { get; set; }
    }
}
