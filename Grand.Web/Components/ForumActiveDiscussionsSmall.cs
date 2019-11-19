using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ForumActiveDiscussionsSmallViewComponent : BaseViewComponent
    {
        private readonly IBoardsViewModelService _boardsViewModelService;

        public ForumActiveDiscussionsSmallViewComponent(IBoardsViewModelService boardsViewModelService)
        {
            _boardsViewModelService = boardsViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _boardsViewModelService.PrepareActiveDiscussions();
            if (model == null)
                return Content("");

            return View(model);

        }
    }
}