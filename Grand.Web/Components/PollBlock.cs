using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Grand.Web.ViewComponents
{
    public class PollBlockViewComponent : BaseViewComponent
    {
        private readonly IPollViewModelService _pollViewModelService;

        public PollBlockViewComponent(IPollViewModelService pollViewModelService)
        {
            this._pollViewModelService = pollViewModelService;
        }

        public IViewComponentResult Invoke(string systemKeyword)
        {
            if (String.IsNullOrWhiteSpace(systemKeyword))
                return Content("");
            var model = _pollViewModelService.PreparePollBySystemName(systemKeyword);
            if (model == null)
                return Content("");

            return View(model);

        }
    }
}