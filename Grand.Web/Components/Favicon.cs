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
            this._commonViewModelService = commonViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await Task.Run(() => _commonViewModelService.PrepareFavicon());
            if (String.IsNullOrEmpty(model.FaviconUrl))
                return Content("");

            return View(model);
        }
    }
}