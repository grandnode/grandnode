using Grand.Services.Forums;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetBoardsIndexHandler : IRequestHandler<GetBoardsIndex, BoardsIndexModel>
    {
        private readonly IForumService _forumService;
        private readonly IMediator _mediator;

        public GetBoardsIndexHandler(
            IForumService forumService,
            IMediator mediator)
        {
            _forumService = forumService;
            _mediator = mediator;
        }

        public async Task<BoardsIndexModel> Handle(GetBoardsIndex request, CancellationToken cancellationToken)
        {
            var forumGroups = await _forumService.GetAllForumGroups();

            var model = new BoardsIndexModel();
            foreach (var forumGroup in forumGroups)
            {
                var forumGroupModel = await _mediator.Send(new GetForumGroup() { ForumGroup = forumGroup });
                model.ForumGroups.Add(forumGroupModel);
            }
            return model;
        }
    }
}
