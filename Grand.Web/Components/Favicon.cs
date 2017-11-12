using Microsoft.AspNetCore.Mvc;
using System;
using Grand.Web.Services;

namespace Grand.Web.ViewComponents
{
    public class FaviconViewComponent : ViewComponent
    {
        private readonly ICommonWebService _commonWebService;

        public FaviconViewComponent(ICommonWebService commonWebService)
        {
            this._commonWebService = commonWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonWebService.PrepareFavicon();
            if (String.IsNullOrEmpty(model.FaviconUrl))
                return Content("");

            return View(model);
        }
    }
}