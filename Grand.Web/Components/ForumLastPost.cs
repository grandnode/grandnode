using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Services.Forums;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class ForumLastPostViewComponent : BaseViewComponent
    {
        private readonly IForumService _forumService;
        private readonly IBoardsWebService _boardsWebService;
        public ForumLastPostViewComponent(IForumService forumService, IBoardsWebService boardsWebService)
        {
            this._forumService = forumService;
            this._boardsWebService = boardsWebService;
        }

        public IViewComponentResult Invoke(string forumPostId, bool showTopic)
        {
            var post = _forumService.GetPostById(forumPostId);
            var model = _boardsWebService.PrepareLastPost(post, showTopic);
            return View(model);

        }
    }
}