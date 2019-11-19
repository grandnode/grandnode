using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class HomePagePollsViewComponent : BaseViewComponent
    {
        private readonly IPollViewModelService _pollViewModelService;

        public HomePagePollsViewComponent(IPollViewModelService pollViewModelService)
        {
            _pollViewModelService = pollViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _pollViewModelService.PrepareHomePagePoll();
            if (!model.Any())
                Content("");
            return View(model);
        }
    }
}