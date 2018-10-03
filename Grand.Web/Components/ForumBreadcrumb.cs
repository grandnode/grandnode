using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class ForumBreadcrumbViewComponent : BaseViewComponent
    {
        private readonly IBoardsViewModelService _boardsViewModelService;
        public ForumBreadcrumbViewComponent(IBoardsViewModelService boardsViewModelService)
        {
            this._boardsViewModelService = boardsViewModelService;
        }

        public IViewComponentResult Invoke(string forumGroupId, string forumId, string forumTopicId)
        {
            var model = _boardsViewModelService.PrepareForumBreadcrumb(forumGroupId, forumId, forumTopicId);
            return View(model);
        }
    }
}