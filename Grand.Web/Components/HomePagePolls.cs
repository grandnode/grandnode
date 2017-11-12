using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class HomePagePollsViewComponent : ViewComponent
    {
        private readonly IPollWebService _pollWebService;

        public HomePagePollsViewComponent(IPollWebService pollWebService)
        {
            this._pollWebService = pollWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _pollWebService.PrepareHomePagePoll();
            if (!model.Any())
                Content("");
            return View(model);
        }
    }
}