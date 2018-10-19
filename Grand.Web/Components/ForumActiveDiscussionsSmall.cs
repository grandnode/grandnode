using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class ForumActiveDiscussionsSmallViewComponent : BaseViewComponent
    {
        private readonly IBoardsViewModelService _boardsViewModelService;

        public ForumActiveDiscussionsSmallViewComponent(IBoardsViewModelService boardsViewModelService)
        {
            this._boardsViewModelService = boardsViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _boardsViewModelService.PrepareActiveDiscussions();
            if (model == null)
                return Content("");

            return View(model);

        }
    }
}