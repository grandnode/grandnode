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
            this._pollViewModelService = pollViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string systemKeyword)
        {
            if (String.IsNullOrWhiteSpace(systemKeyword))
                return Content("");
            var model = await Task.Run(() => _pollViewModelService.PreparePollBySystemName(systemKeyword));
            if (model == null)
                return Content("");

            return View(model);

        }
    }
}