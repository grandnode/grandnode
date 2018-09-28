using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Services.Forums;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class ForumBreadcrumbViewComponent : BaseViewComponent
    {
        private readonly IForumService _forumService;
        private readonly IBoardsViewModelService _boardsViewModelService;
        public ForumBreadcrumbViewComponent(IForumService forumService, IBoardsViewModelService boardsViewModelService)
        {
            this._forumService = forumService;
            this._boardsViewModelService = boardsViewModelService;
        }

        public IViewComponentResult Invoke(string forumGroupId, string forumId, string forumTopicId)
        {
            var model = _boardsViewModelService.PrepareForumBreadcrumb(forumGroupId, forumId, forumTopicId);
            return View(model);
        }
    }
}