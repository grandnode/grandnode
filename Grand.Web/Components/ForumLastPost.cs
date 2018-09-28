using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Services.Forums;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class ForumLastPostViewComponent : BaseViewComponent
    {
        private readonly IForumService _forumService;
        private readonly IBoardsViewModelService _boardsViewModelService;
        public ForumLastPostViewComponent(IForumService forumService, IBoardsViewModelService boardsViewModelService)
        {
            this._forumService = forumService;
            this._boardsViewModelService = boardsViewModelService;
        }

        public IViewComponentResult Invoke(string forumPostId, bool showTopic)
        {
            var post = _forumService.GetPostById(forumPostId);
            var model = _boardsViewModelService.PrepareLastPost(post, showTopic);
            return View(model);

        }
    }
}