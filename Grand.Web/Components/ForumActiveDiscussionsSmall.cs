using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.Forums;

namespace Grand.Web.ViewComponents
{
    public class ForumActiveDiscussionsSmallViewComponent : ViewComponent
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