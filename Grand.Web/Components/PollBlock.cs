using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class PollBlockViewComponent : BaseViewComponent
    {
        private readonly IPollViewModelService _pollViewModelService;

        public PollBlockViewComponent(IPollViewModelService pollViewModelService)
        {
            _pollViewModelService = pollViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string systemKeyword)
        {
            if (string.IsNullOrWhiteSpace(systemKeyword))
                return Content("");
            var model = await _pollViewModelService.PreparePollBySystemName(systemKeyword);
            if (model == null)
                return Content("");

            return View(model);

        }
    }
}