using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetActiveDiscussions : IRequest<ActiveDiscussionsModel>
    {
    }
}
