using Grand.Core.Domain.News;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
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
            _newsViewModelService = newsViewModelService;
            _newsSettings = newsSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
                return Content("");

            var model = await _newsViewModelService.PrepareHomePageNewsItems();
            if (!model.NewsItems.Any())
                return Content("");

            return View(model);
        }
    }
}