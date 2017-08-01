using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.News;

namespace Grand.Web.ViewComponents
{
    public class HomePageNewsViewComponent : ViewComponent
    {
        private readonly INewsWebService _newsWebService;
        private readonly NewsSettings _newsSettings;
        public HomePageNewsViewComponent(INewsWebService newsWebService,
            NewsSettings newsSettings)
        {
            this._newsWebService = newsWebService;
            this._newsSettings = newsSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
                return Content("");

            var model = _newsWebService.PrepareHomePageNewsItems();
            return View(model);
        }
    }
}