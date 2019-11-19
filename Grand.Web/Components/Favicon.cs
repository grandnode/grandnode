using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class FaviconViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public FaviconViewComponent(ICommonViewModelService commonViewModelService)
        {
            _commonViewModelService = commonViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonViewModelService.PrepareFavicon();
            if (string.IsNullOrEmpty(model.FaviconUrl))
                return Content("");

            return View(model);
        }
    }
}