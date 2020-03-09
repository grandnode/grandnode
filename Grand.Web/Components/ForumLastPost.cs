using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ForumLastPostViewComponent : BaseViewComponent
    {
        private readonly IBoardsViewModelService _boardsViewModelService;
        public ForumLastPostViewComponent(IBoardsViewModelService boardsViewModelService)
        {
            _boardsViewModelService = boardsViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string forumPostId, bool showTopic)
        {
            var model = await _boardsViewModelService.PrepareLastPost(forumPostId, showTopic);
            return View(model);
        }
    }
}