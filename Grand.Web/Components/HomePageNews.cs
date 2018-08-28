using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Core.Domain.News;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class HomePageNewsViewComponent : BaseViewComponent
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