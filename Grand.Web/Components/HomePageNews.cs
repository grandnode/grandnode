using Grand.Core.Domain.News;
using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class HomePageNewsViewComponent : BaseViewComponent
    {
        private readonly INewsViewModelService _newsViewModelService;
        private readonly NewsSettings _newsSettings;
        public HomePageNewsViewComponent(INewsViewModelService newsViewModelService,
            NewsSettings newsSettings)
        {
            this._newsViewModelService = newsViewModelService;
            this._newsSettings = newsSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
                return Content("");

            var model = await Task.Run(() => _newsViewModelService.PrepareHomePageNewsItems());
            return View(model);
        }
    }
}