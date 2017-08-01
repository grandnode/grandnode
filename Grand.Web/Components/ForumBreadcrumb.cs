using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.Forums;
using Grand.Services.Forums;

namespace Grand.Web.ViewComponents
{
    public class ForumBreadcrumbViewComponent : ViewComponent
    {
        private readonly IForumService _forumService;
        private readonly IBoardsWebService _boardsWebService;
        public ForumBreadcrumbViewComponent(IForumService forumService, IBoardsWebService boardsWebService)
        {
            this._forumService = forumService;
            this._boardsWebService = boardsWebService;
        }

        public IViewComponentResult Invoke(string forumGroupId, string forumId, string forumTopicId)
        {
            var model = _boardsWebService.PrepareForumBreadcrumb(forumGroupId, forumId, forumTopicId);
            return View(model);
        }
    }
}