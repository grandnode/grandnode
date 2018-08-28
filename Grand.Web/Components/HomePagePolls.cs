using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class HomePagePollsViewComponent : BaseViewComponent
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