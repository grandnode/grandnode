using Microsoft.AspNetCore.Mvc;
using System;
using Grand.Web.Services;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class PollBlockViewComponent : BaseViewComponent
    {
        private readonly IPollWebService _pollWebService;

        public PollBlockViewComponent(IPollWebService pollWebService)
        {
            this._pollWebService = pollWebService;
        }

        public IViewComponentResult Invoke(string systemKeyword)
        {
            if (String.IsNullOrWhiteSpace(systemKeyword))
                return Content("");
            var model = _pollWebService.PreparePollBySystemName(systemKeyword);
            if (model == null)
                return Content("");

            return View(model);

        }
    }
}