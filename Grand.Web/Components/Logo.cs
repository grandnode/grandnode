using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class LogoViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public LogoViewComponent(ICommonViewModelService commonViewModelService)
        {
            this._commonViewModelService = commonViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonViewModelService.PrepareLogo();
            return View(model);
        }
    }
}