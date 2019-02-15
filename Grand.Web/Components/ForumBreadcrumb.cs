using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ForumBreadcrumbViewComponent : BaseViewComponent
    {
        private readonly IBoardsViewModelService _boardsViewModelService;
        public ForumBreadcrumbViewComponent(IBoardsViewModelService boardsViewModelService)
        {
            this._boardsViewModelService = boardsViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string forumGroupId, string forumId, string forumTopicId)
        {
            var model = await Task.Run(() => _boardsViewModelService.PrepareForumBreadcrumb(forumGroupId, forumId, forumTopicId));
            return View(model);
        }
    }
}