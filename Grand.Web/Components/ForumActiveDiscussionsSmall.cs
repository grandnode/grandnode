using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class ForumActiveDiscussionsSmallViewComponent : BaseViewComponent
    {
        private readonly IBoardsWebService _boardsWebService;

        public ForumActiveDiscussionsSmallViewComponent(IBoardsWebService boardsWebService)
        {
            this._boardsWebService = boardsWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _boardsWebService.PrepareActiveDiscussions();
            if (model == null)
                return Content("");

            return View(model);

        }
    }
}