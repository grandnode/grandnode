using Microsoft.AspNetCore.Mvc;
using System;
using Grand.Web.Services;
using Grand.Framework.Components;

namespace Grand.Web.ViewComponents
{
    public class FaviconViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public FaviconViewComponent(ICommonViewModelService commonViewModelService)
        {
            this._commonViewModelService = commonViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonViewModelService.PrepareFavicon();
            if (String.IsNullOrEmpty(model.FaviconUrl))
                return Content("");

            return View(model);
        }
    }
}